using System.Text.Json;
using Application.Abstractions.Kafka;
using Core;
using Dapper;
using Infrastructure.Options.Outbox;
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
    NpgsqlDataSource dataSource,
    IEventPublisher eventPublisher,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> outboxOptions) : IOutboxProcessor
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: outboxOptions.Value.MaxRetryAttempts,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(outboxOptions.Value.RetryIntervalSeconds),
            onRetry: (ex, delay, attempt, context) => 
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
            var messages = await GetPendingMessagesAsync(connection, transaction, batchSize, cancellationToken);

            var outboxMessages = messages as OutboxMessage[] ?? messages.ToArray();
            if (!outboxMessages.Any())
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

    private static async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await connection.QueryAsync<OutboxMessage>(
            new CommandDefinition(
                """
                SELECT * FROM outbox_messages 
                WHERE processed_on_utc IS NULL 
                ORDER BY occurred_on_utc
                FOR UPDATE SKIP LOCKED
                LIMIT @batchSize
                """,
                new { batchSize },
                transaction: transaction,
                cancellationToken: cancellationToken));
    }

    private async Task ProcessSingleMessageAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var (eventType, integrationEvent) = DeserializeMessage(message);

            await _retryPolicy.ExecuteAsync(async (context) => 
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
        var updated = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE outbox_messages 
                SET processed_on_utc = @now 
                WHERE id = @id AND processed_on_utc IS NULL
                """,
                new { now = DateTime.UtcNow, id = messageId },
                transaction: transaction,
                cancellationToken: cancellationToken));

        if (updated == 0)
        {
            logger.LogWarning("Message {MessageId} was already processed by another instance", messageId);
        }
    }

    private async Task HandleProcessingErrorAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Failed to process outbox message {MessageId}", messageId);

        var attemptCount = await GetAttemptCountAsync(connection, transaction, messageId, cancellationToken);

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
        NpgsqlTransaction transaction,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        return await connection.QuerySingleOrDefaultAsync<int>(
            new CommandDefinition(
                "SELECT error_count FROM outbox_messages WHERE id = @id",
                new { id = messageId },
                transaction: transaction,
                cancellationToken: cancellationToken));
    }

    private async Task RecordFailedAttemptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE outbox_messages 
                SET 
                    error = @error,
                    error_count = error_count + 1
                WHERE id = @id
                """,
                new 
                { 
                    error = $"{exception.GetType().Name}: {exception.Message}",
                    id = messageId
                },
                transaction: transaction,
                cancellationToken: cancellationToken));
    }

    private async Task MoveToDeadLetterQueueAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO dead_letter_messages
                (id, topic, type, content, error, original_occurred_on, archived_at)
                SELECT 
                    id, 
                    topic, 
                    type, 
                    content, 
                    error, 
                    occurred_on_utc,
                    @archivedAt
                FROM outbox_messages
                WHERE id = @id
                """,
                new 
                { 
                    id = messageId,
                    archivedAt = DateTime.UtcNow
                },
                transaction: transaction,
                cancellationToken: cancellationToken));
    }
}