using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Infrastructure.Messaging.Realtime;

namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqRealtimePublisher : IRealtimePublisher
{
    private readonly IRabbitMqConnection _connection;

    public RabbitMqRealtimePublisher(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public async Task PublishAsync(RealtimeHubType hubType, string targetGroup, string action, object payload, CancellationToken cancellationToken = default)
    {
        using var channel = await _connection.CreateChannelAsync(cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqConstants.RealtimeUpdatesExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        
        var serializedPayload = JsonSerializer.Serialize(payload);

        var message = new RealtimeUpdateMessage(hubType, targetGroup, action, serializedPayload);
        
        var serializedMessage = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(serializedMessage);

        await channel.BasicPublishAsync(
            exchange: RabbitMqConstants.RealtimeUpdatesExchange,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }
}