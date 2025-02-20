using System.Data;
using Infrastructure.Options.Db;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.HealthChecks;

public class PostgresHealthCheck(INpgsqlConnectionFactory npgsqlConnectionFactory, IOptions<PostgresOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = npgsqlConnectionFactory.CreateConnection(options.Value.GetConnectionString());
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            
            command.ExecuteScalar();

            return HealthCheckResult.Healthy("PostgreSQL instance is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL instance is unhealthy.", ex);
        }
    }
}

public interface INpgsqlConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);
}

public class NpgsqlConnectionFactory : INpgsqlConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }
}