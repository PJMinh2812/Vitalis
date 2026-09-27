using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Vitalis.Application.Interfaces;

namespace Vitalis.Infrastructure.Caching;

// Cache-aside is an optimization, not a correctness requirement — a Redis
// outage must degrade to "always miss"/"no-op", never break the underlying
// read (specialties/services would otherwise 500 whenever Redis is down).
// Checking IsConnected first (a local, in-memory flag) avoids paying Redis's
// ~5s per-operation timeout on every single request while it's down — only
// the very first call after an outage starts pays that cost.
public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!connectionMultiplexer.IsConnected)
            return default;

        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis GetAsync failed for key {Key}, treating as cache miss", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        if (!connectionMultiplexer.IsConnected)
            return;

        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry }, cancellationToken);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis SetAsync failed for key {Key}, skipping cache write", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!connectionMultiplexer.IsConnected)
            return;

        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis RemoveAsync failed for key {Key}", key);
        }
    }
}
