using Marketplace.Application.Common.Interfaces;
using Marketplace.Infrastructure.Caching;
using Marketplace.Infrastructure.Common;
using Marketplace.Infrastructure.Identity;
using Marketplace.Infrastructure.Messaging;
using Marketplace.Infrastructure.Outbox;
using Marketplace.Infrastructure.Persistence;
using Marketplace.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Marketplace.Infrastructure;

/// <summary>
/// Infrastructure-layer composition root. Wires PostgreSQL/EF Core, Redis, RabbitMQ, JWT
/// authentication + permission-based authorization, and the Outbox background processor.
/// Called once from Marketplace.API's Program.cs, after AddApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddCaching(services, configuration);
        AddMessaging(services, configuration);
        AddIdentityInfrastructure(services, configuration);
        AddOutbox(services);

        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<SoftDeleteSaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisCacheOptions>(configuration.GetSection(RedisCacheOptions.SectionName));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = configuration[$"{RedisCacheOptions.SectionName}:InstanceName"] ?? "marketplace:";
        });

        services.AddScoped<ICacheService, RedisCacheService>();
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventBusPublisher, RabbitMqEventBusPublisher>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
    }

    private static void AddIdentityInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PasswordHashingOptions>(configuration.GetSection(PasswordHashingOptions.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, Argon2idPasswordHasher>();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"] ?? "Marketplace.API",
                ValidAudience = jwtSection["Audience"] ?? "Marketplace.Clients",
                // RS256 (ADR-014): validation only ever needs the PUBLIC key — this process
                // never needs signing capability just to accept incoming tokens. A
                // missing/invalid public key file throws a clear InvalidOperationException
                // (naming the exact config key to fix) rather than a cryptic downstream
                // JWT-validation failure.
                IssuerSigningKey = new RsaSecurityKey(LoadPublicKey(jwtSection["PublicKeyPath"])),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddAuthorization();
    }

    private static System.Security.Cryptography.RSA LoadPublicKey(string? publicKeyPath)
    {
        if (string.IsNullOrWhiteSpace(publicKeyPath) || !File.Exists(publicKeyPath))
        {
            throw new InvalidOperationException(
                $"JWT public key not found at '{publicKeyPath}'. Set Jwt:PublicKeyPath " +
                "(JWT_PUBLIC_KEY_PATH) to a PEM-encoded RSA public key file matching the private key.");
        }

        var rsa = System.Security.Cryptography.RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
        return rsa;
    }

    private static void AddOutbox(IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessorBackgroundService>();
    }
}
