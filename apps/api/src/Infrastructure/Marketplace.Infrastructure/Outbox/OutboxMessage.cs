namespace Marketplace.Infrastructure.Outbox;

/// <summary>
/// Outbox row — written in the same DB transaction as the business change that caused it
/// (Architecture doc ADR-005). A separate background processor polls for
/// ProcessedAt == null rows and publishes them to RabbitMQ, then stamps ProcessedAt.
/// This guarantees "event was persisted" and "business change was persisted" always
/// succeed or fail together, without a distributed transaction.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>CLR type name of the IntegrationEvent, used to deserialize + as the routing key suffix.</summary>
    public string Type { get; set; } = default!;

    /// <summary>JSON-serialized IntegrationEvent payload.</summary>
    public string Content { get; set; } = default!;

    public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
