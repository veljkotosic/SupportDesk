using RabbitMQ.Client;

namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqConnection
{
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}