using Application.Abstractions.Kafka;
using Dapper;
using Infrastructure.Options.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Outbox;

public class OutboxCleanupService(
    NpgsqlDataSource dataSource,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> outboxOptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Outbox cleanup service started.");
            
            await using var connection = await dataSource.OpenConnectionAsync(stoppingToken);
            
            await connection.ExecuteAsync(
                """
                DELETE FROM outbox_messages
                WHERE processed_on_utc IS NOT NULL
                AND processed_on_utc < @cutoffDate
                """,
                new { cutoffDate = DateTime.UtcNow.AddDays(-outboxOptions.Value.RetentionDays) });
                
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            
            logger.LogInformation("Outbox cleanup service stopped.");
        }
        
        
    }
}