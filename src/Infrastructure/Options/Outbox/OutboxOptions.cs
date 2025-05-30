using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options.Outbox;

public class OutboxOptions
{
    [Required]
    public TimeSpan PollingInterval { get; set; }
    [Required, Range(1, 5000)]
    public int BatchSize { get; set; }
    [Required, Range(1, 50)]
    public int RetentionDays { get; set; }
    [Required, Range(1, 10)]
    public int MaxRetryAttempts { get; set; }
    [Required, Range(1, 10)]
    public int RetryIntervalSeconds { get; set; }
}