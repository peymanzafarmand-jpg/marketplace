using System.Text.Json;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Common;
using Marketplace.Infrastructure.Persistence;

namespace Marketplace.Infrastructure.Outbox;

/// <summary>
/// Implements IIntegrationEventPublisher by inserting an OutboxMessage row via the current
/// DbContext (so it lands in whatever transaction the calling handler is already in — the
/// caller must still call SaveChanges). No RabbitMQ call happens here; see
/// OutboxProcessorBackgroundService for the actual publish step.
/// </summary>
public class OutboxIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly ApplicationDbContext _context;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OutboxIntegrationEventPublisher(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var message = new OutboxMessage
        {
            Type = integrationEvent.EventType,
            Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
            OccurredOn = integrationEvent.OccurredOn
        };

        _context.OutboxMessages.Add(message);
        return Task.CompletedTask;
    }
}
