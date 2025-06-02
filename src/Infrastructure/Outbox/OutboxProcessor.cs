using System.Text.Json;
using Application.Abstractions.Kafka;
using Core;
using Infrastructure.Options.Outbox;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;
using Polly.Retry;

namespace Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken);
}

public class OutboxProcessor(
    IOutboxRepository outboxRepository,
    IEventPublisher eventPublisher,
    NpgsqlDataSource dataSource,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> outboxOptions) : IOutboxProcessor
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: outboxOptions.Value.MaxRetryAttempts,
            sleepDurationProvider: _ => TimeSpan.FromSeconds(outboxOptions.Value.RetryIntervalSeconds),
            onRetry: (ex, _, attempt, context) => 
            {
                logger.LogWarning(ex, 
                    "Retry attempt {Attempt}/{MaxAttempts} for message {MessageId}",
                    attempt, outboxOptions.Value.MaxRetryAttempts, context["MessageId"]);
            });
    
    public async Task ProcessPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var messages = await GetPendingMessagesAsync(batchSize, cancellationToken);

            var outboxMessages = messages as OutboxMessage[] ?? messages.ToArray();
            if (outboxMessages.Length == 0)
            {
                logger.LogDebug("No pending outbox messages found.");
                return;
            }

            logger.LogInformation("Processing {MessageCount} outbox messages", outboxMessages.Length);

            foreach (var message in outboxMessages)
            {
                await ProcessSingleMessageAsync(connection, transaction, message, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Error processing outbox batch.");
            throw;
        }
    }

    private async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        var result = await outboxRepository.GetPendingAsync(batchSize, cancellationToken);
        
        return result;
    }

    private async Task ProcessSingleMessageAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var (_, integrationEvent) = DeserializeMessage(message);

            await _retryPolicy.ExecuteAsync(async _ => 
            {
                await eventPublisher.PublishAsync(message.Topic, integrationEvent, cancellationToken);
            }, new Context { { "MessageId", message.Id.ToString() } });

            await MarkMessageAsProcessedAsync(connection, transaction, message.Id, cancellationToken);

            logger.LogDebug("Successfully processed outbox message {MessageId}", message.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await HandleProcessingErrorAsync(
                connection, 
                transaction, 
                message.Id, 
                ex, 
                cancellationToken);
        }
    }

    private (Type eventType, IIntegrationEvent integrationEvent) DeserializeMessage(OutboxMessage message)
    {
        var eventType = Type.GetType(message.Type);
        if (eventType == null || !typeof(IIntegrationEvent).IsAssignableFrom(eventType))
        {
            throw new InvalidOperationException(
                $"Type {message.Type} not found or doesn't implement IIntegrationEvent");
        }

        var deserialized = JsonSerializer.Deserialize(message.Content, eventType);
        if (deserialized is not IIntegrationEvent integrationEvent)
        {
            throw new InvalidOperationException("Deserialized message is not an IIntegrationEvent");
        }

        return (eventType, integrationEvent);
    }

    private async Task MarkMessageAsProcessedAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId, 
        CancellationToken cancellationToken)
    {
        await outboxRepository.MarkAsProcessedAsync(connection, transaction, messageId, cancellationToken);
    }

    private async Task HandleProcessingErrorAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Failed to process outbox message {MessageId}", messageId);

        var attemptCount = await GetAttemptCountAsync(connection, messageId, cancellationToken);

        if (attemptCount >= outboxOptions.Value.MaxRetryAttempts - 1)
        {
            await MoveToDeadLetterQueueAsync(connection, transaction, messageId, cancellationToken);
            await MarkMessageAsProcessedAsync(connection, transaction, messageId, cancellationToken);
            logger.LogWarning("Message {MessageId} moved to dead letter queue after {AttemptCount} attempts", 
                messageId, attemptCount + 1);
        }
        else
        {
            await RecordFailedAttemptAsync(
                connection, 
                transaction, 
                messageId, 
                exception, 
                cancellationToken);
        }
    }

    private async Task<int> GetAttemptCountAsync(
        NpgsqlConnection connection,
        Guid messageId, 
        CancellationToken cancellationToken)
    {
        var result = await outboxRepository.GetAttemptCountAsync(connection, messageId, cancellationToken);
        
        return result;
    }

    private async Task RecordFailedAttemptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await outboxRepository.RecordFailedAttemptAsync(connection, transaction, messageId, $"{exception.GetType().Name}: {exception.Message}", cancellationToken);
    }

    private async Task MoveToDeadLetterQueueAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        await outboxRepository.MoveToDeadLetterQueueAsync(connection, transaction, messageId, cancellationToken);
    }
}