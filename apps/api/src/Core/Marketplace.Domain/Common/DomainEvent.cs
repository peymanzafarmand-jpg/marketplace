using MediatR;

namespace Marketplace.Domain.Common;

/// <summary>
/// Base type for in-process domain events, dispatched via MediatR after SaveChanges
/// succeeds (see Infrastructure DispatchDomainEventsInterceptor). For events that must
/// cross module/service boundaries, raise a matching IntegrationEvent through the Outbox
/// instead (see Marketplace.Application.Common / Infrastructure.Outbox).
/// </summary>
public abstract class DomainEvent : INotification
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
