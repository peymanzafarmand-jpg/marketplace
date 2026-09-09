using Marketplace.Infrastructure.Messaging;
using Marketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Outbox;

/// <summary>
/// Polls unprocessed OutboxMessage rows and publishes each to RabbitMQ via
/// IEventBusPublisher, then marks it processed. Foundation-sprint implementation: fixed
/// polling interval, simple retry counter. Sufficient until real integration events exist
/// (there are none yet — this sprint only builds the pipe).
/// </summary>
public class OutboxProcessorBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;

    public OutboxProcessorBackgroundService(
        IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox processing loop failed");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBusPublisher>();

        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await eventBus.PublishAsync(message.Type, message.Content, cancellationToken);
                message.ProcessedOn = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} (attempt {Attempt})",
                    message.Id, message.RetryCount);
            }
        }

        if (messages.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }
}
