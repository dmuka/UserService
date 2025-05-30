using System.Text.Json;
using Dapper;
using Infrastructure.Outbox;
using Npgsql;

namespace Infrastructure.Repositories;

public class OutboxRepository
{
    public async Task AddToOutbox<T>(T message, string topic, NpgsqlDataSource dataSource)
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
}