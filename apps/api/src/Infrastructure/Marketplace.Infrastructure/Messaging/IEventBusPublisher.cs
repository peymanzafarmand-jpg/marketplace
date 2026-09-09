namespace Marketplace.Infrastructure.Messaging;

/// <summary>Thin publish abstraction so the Outbox processor doesn't depend on RabbitMQ.Client directly.</summary>
public interface IEventBusPublisher
{
    Task PublishAsync(string eventType, string jsonPayload, CancellationToken cancellationToken = default);
}
