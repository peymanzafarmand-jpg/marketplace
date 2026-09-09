using Marketplace.Domain.Common;

namespace Marketplace.Application.Common.Interfaces;

/// <summary>
/// Writes an IntegrationEvent into the Outbox table in the SAME transaction as the
/// triggering write. A separate background processor (Infrastructure.Outbox) later reads
/// unpublished rows and publishes them to RabbitMQ — see Architecture doc ADR-005.
/// Application code should call this instead of talking to RabbitMQ directly.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
