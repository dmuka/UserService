using Infrastructure.Options.Outbox;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Outbox;

public class OutboxCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> outboxOptions) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Outbox cleanup service started.");

            using var scope = scopeFactory.CreateScope();
            
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            
            await outboxRepository.CleanUpAsync(outboxOptions.Value.RetentionDays, stoppingToken);
                
            await Task.Delay(TimeSpan.FromHours(outboxOptions.Value.CleanupPauseHours), stoppingToken);
            
            logger.LogInformation("Outbox cleanup service stopped.");
        }
        
        
    }
}