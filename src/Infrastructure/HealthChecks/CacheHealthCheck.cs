using Domain.Roles;
using Infrastructure.Caching.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class CacheHealthCheck(ICacheService cache) : IHealthCheck
{
    private const string Name = "health";
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = Role.Create(Guid.CreateVersion7(), Name).Value;
            
            cache.Create(Name, role);

            _ = cache.GetEntity<Role>(Name);
            
            cache.Remove(Name);
            
            return HealthCheckResult.Healthy("Cache instance is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cache instance is unhealthy.", ex);
        }
    }
}