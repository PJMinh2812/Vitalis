using Microsoft.Extensions.Diagnostics.HealthChecks;
using Vitalis.Infrastructure.Persistence;

namespace Vitalis.Infrastructure.HealthChecks;

// Custom check instead of the community AspNetCore.HealthChecks.SqlServer
// package — avoids a package just for one CanConnectAsync call.
public class DatabaseHealthCheck(VitalisDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("SQL Server reachable")
                : HealthCheckResult.Unhealthy("SQL Server not reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server check threw", ex);
        }
    }
}
