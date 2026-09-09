using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Marketplace.Infrastructure.Messaging;

/// <summary>
/// Publishes to a topic exchange so future consumers (Notifications, Reporting, a future
/// extracted microservice) can each bind their own queue without this publisher knowing
/// who's listening — see Architecture doc section 15 on future extraction.
/// </summary>
public class RabbitMqEventBusPublisher : IEventBusPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBusPublisher> _logger;
    private readonly Lazy<IConnection> _connection;

    public RabbitMqEventBusPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventBusPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _connection = new Lazy<IConnection>(CreateConnection);
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection("marketplace-api");
    }

    public Task PublishAsync(string eventType, string jsonPayload, CancellationToken cancellationToken = default)
    {
        using var channel = _connection.Value.CreateModel();

        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(jsonPayload);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = eventType;

        channel.BasicPublish(_options.ExchangeName, routingKey: eventType, basicProperties: properties, body: body);

        _logger.LogDebug("Published integration event {EventType} to {Exchange}", eventType, _options.ExchangeName);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _connection.Value.Dispose();
    }
}
