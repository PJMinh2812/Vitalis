using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Vitalis.Infrastructure.HealthChecks;

// Custom check instead of the community AspNetCore.HealthChecks.Redis package —
// StackExchange.Redis is already a transitive dependency of the cache package.
public class RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await connectionMultiplexer.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"Redis reachable ({latency.TotalMilliseconds:F0} ms)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis check threw", ex);
        }
    }
}
