using Infrastructure.Options.Db;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.HealthChecks;

public class PostgresHealthCheck(IOptions<PostgresOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(options.Value.GetConnectionString());
            await connection.OpenAsync(cancellationToken);
            
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("PostgreSQL instance is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL instance is unhealthy.", ex);
        }
    }
}