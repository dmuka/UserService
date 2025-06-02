using System.Text.Json;
using Dapper;
using Infrastructure.Outbox;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Infrastructure.Repositories;

public interface IOutboxRepository
{
    Task Add<T>(T message, string topic);
    Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);
    Task MarkAsProcessedAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid messageId, CancellationToken cancellationToken);
    Task<int> GetAttemptCountAsync(NpgsqlConnection connection, Guid messageId, CancellationToken cancellationToken);
    Task RecordFailedAttemptAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid messageId, string error, CancellationToken cancellationToken);
    Task MoveToDeadLetterQueueAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid messageId, CancellationToken cancellationToken);
    Task CleanUpAsync(int retentionDays, CancellationToken cancellationToken);
}

public class OutboxRepository(NpgsqlDataSource dataSource, ILogger<OutboxRepository> logger) : IOutboxRepository
{
    public async Task Add<T>(T message, string topic)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).FullName ?? string.Empty,
            Content = JsonSerializer.Serialize(message)
        };

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(
            """
                    INSERT INTO outbox_messages 
                        (id, occurred_on_utc, type, content)
                    VALUES 
                        (@Id, @OccurredOnUtc, @Type, @Content::jsonb)
            """,
            outboxMessage);
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var messages = await connection.QueryAsync<OutboxMessage>(
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

            await transaction.CommitAsync(cancellationToken);
            return messages;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task MarkAsProcessedAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId, 
        CancellationToken cancellationToken)
    {
        try
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

            await transaction.CommitAsync(cancellationToken);

            if (updated == 0)
            {
                logger.LogWarning("Message {MessageId} was already processed by another instance", messageId);
            }
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> GetAttemptCountAsync(
        NpgsqlConnection connection,
        Guid messageId, 
        CancellationToken cancellationToken)
    {
        return await connection.QuerySingleOrDefaultAsync<int>(
            new CommandDefinition(
                "SELECT error_count FROM outbox_messages WHERE id = @id",
                new { id = messageId },
                cancellationToken: cancellationToken));
    }

    public async Task RecordFailedAttemptAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId, 
        string error, 
        CancellationToken cancellationToken)
    {
        try
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
                    new { error, id = messageId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task MoveToDeadLetterQueueAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid messageId, 
        CancellationToken cancellationToken)
    {
        try
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
                    new { id = messageId, archivedAt = DateTime.UtcNow },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CleanUpAsync(int retentionDays, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            
        await connection.ExecuteAsync(
            """
            DELETE FROM outbox_messages
            WHERE processed_on_utc IS NOT NULL
            AND processed_on_utc < @cutoffDate
            """,
            new { cutoffDate = DateTime.UtcNow.AddDays(-retentionDays) });
    }
}