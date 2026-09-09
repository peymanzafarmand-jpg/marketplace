using System.Security.Cryptography;
using Marketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace Marketplace.IntegrationTests;

public class MarketplaceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("marketplace_test")
        .WithUsername("marketplace")
        .WithPassword("marketplace_test_only")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13-management-alpine")
        .WithUsername("marketplace")
        .WithPassword("marketplace_test_only")
        .Build();

    private readonly string _tempKeyDir = Directory.CreateTempSubdirectory("marketplace-test-jwt-").FullName;
    private string PrivateKeyPath => Path.Combine(_tempKeyDir, "private.pem");
    private string PublicKeyPath => Path.Combine(_tempKeyDir, "public.pem");

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                ["RabbitMq:HostName"] = _rabbitMq.Hostname,
                ["RabbitMq:Port"] = _rabbitMq.GetMappedPublicPort(5672).ToString(),
                ["RabbitMq:UserName"] = "marketplace",
                ["RabbitMq:Password"] = "marketplace_test_only",
                ["Jwt:PrivateKeyPath"] = PrivateKeyPath,
                ["Jwt:PublicKeyPath"] = PublicKeyPath,
                ["PasswordHashing:Pepper"] = "integration-test-pepper-not-for-production-use-only"
            });
        });

        builder.ConfigureServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        });
    }

    private void GenerateTestKeyPair()
    {
        using var rsa = RSA.Create(2048);
        File.WriteAllText(PrivateKeyPath, rsa.ExportRSAPrivateKeyPem());
        File.WriteAllText(PublicKeyPath, rsa.ExportRSAPublicKeyPem());
    }

    public async Task InitializeAsync()
    {
        GenerateTestKeyPair();
        await _postgres.StartAsync();
        await _redis.StartAsync();
        await _rabbitMq.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await _rabbitMq.DisposeAsync();
        Directory.Delete(_tempKeyDir, recursive: true);
        await base.DisposeAsync();
    }
}
