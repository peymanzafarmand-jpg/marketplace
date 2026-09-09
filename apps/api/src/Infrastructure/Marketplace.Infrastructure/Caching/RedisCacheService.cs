using System.Text.Json;
using Marketplace.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Marketplace.Infrastructure.Caching;

/// <summary>
/// Implements ICacheService on top of IDistributedCache (StackExchange.Redis provider,
/// registered in DependencyInjection.cs). JSON-serializes values; callers just deal in
/// plain POCOs, never in Redis/serialization concerns.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
    }

    public async Task SetAsync<T>(
        string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        };

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(key, cancellationToken);
}
