using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SupportDesk.Infrastructure.Messaging.Outbox;

namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqDomainEventPublisher : IDomainEventPublisher
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqDomainEventPublisher(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {       
        using var channel = await _connection.CreateChannelAsync(cancellationToken);
        
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqConstants.DomainEventsExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        
        var routingKey = message.EventType;

        await channel.BasicPublishAsync(
            exchange: RabbitMqConstants.DomainEventsExchange,
            routingKey: routingKey,
            body: body,
            cancellationToken: cancellationToken);
    }
}