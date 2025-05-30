using Infrastructure.Options.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Infrastructure.Outbox;

public class OutboxPublisher(
    IServiceProvider serviceProvider,
    ILogger<OutboxPublisher> logger,
    IOptions<OutboxOptions> outboxOptions) : BackgroundService
{
    private readonly TimeSpan _pollingInterval = outboxOptions.Value.PollingInterval;
    private readonly int _batchSize = outboxOptions.Value.BatchSize;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox polling publisher started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                
                await processor.ProcessPendingMessagesAsync(_batchSize, stoppingToken);
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                logger.LogError(ex, "Error processing outbox messages.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
        
        logger.LogInformation("Outbox polling publisher stopped.");
    }
}