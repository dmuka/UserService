﻿namespace Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Topic { get; init; }
    public required string Content { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; init; }
    public string? Error { get; init; }
}