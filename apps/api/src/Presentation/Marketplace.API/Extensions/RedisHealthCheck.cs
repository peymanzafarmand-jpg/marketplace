using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Marketplace.API.Extensions;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IConfiguration configuration) => _configuration = configuration;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Redis") ?? "localhost:6379";

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 3000;
            options.AbortOnConnectFail = true;

            await using var connection = await ConnectionMultiplexer.ConnectAsync(options);
            var pong = await connection.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"Redis responded in {pong.TotalMilliseconds}ms.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Could not connect to Redis.", exception);
        }
    }
}
