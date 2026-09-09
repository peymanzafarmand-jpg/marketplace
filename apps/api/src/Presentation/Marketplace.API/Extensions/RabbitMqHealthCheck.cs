using Marketplace.Infrastructure.Messaging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Marketplace.API.Extensions;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RabbitMqHealthCheck(IConfiguration configuration) => _configuration = configuration;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var section = _configuration.GetSection(RabbitMqOptions.SectionName);

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = section["HostName"] ?? "localhost",
                Port = int.TryParse(section["Port"], out var port) ? port : 5672,
                UserName = section["UserName"] ?? "guest",
                Password = section["Password"] ?? "guest",
                VirtualHost = section["VirtualHost"] ?? "/",
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
            };

            using var connection = factory.CreateConnection("marketplace-api-healthcheck");
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Could not connect to RabbitMQ.", exception));
        }
    }
}
