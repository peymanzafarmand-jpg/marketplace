using HealthChecks.UI.Client;
using Marketplace.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Marketplace.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddMarketplaceHealthChecks(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("postgres", tags: new[] { "ready" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" })
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: new[] { "ready" });

        return services;
    }

    public static void MapMarketplaceHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
}
