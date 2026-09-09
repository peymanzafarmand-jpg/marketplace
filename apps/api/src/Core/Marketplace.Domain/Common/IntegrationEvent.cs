namespace Marketplace.Domain.Common;

/// <summary>
/// Base type for events published OUTSIDE the process boundary (via the Outbox → RabbitMQ).
/// Unlike DomainEvent (in-process, MediatR), an IntegrationEvent is a serialization contract:
/// keep it flat, versioned by name, and free of domain entity references.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Used as the Outbox "Type" column and the RabbitMQ routing key suffix.</summary>
    public abstract string EventType { get; }
}
