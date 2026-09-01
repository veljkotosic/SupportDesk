using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;

namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

internal sealed class RabbitMqConnection : IRabbitMqConnection, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;

    public RabbitMqConnection(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            await CreateConnectionAsync();
        }

        return await _connection!.CreateChannelAsync(null, cancellationToken);
    }
    
    public async Task CreateConnectionAsync()
    {
        var factory = CreateConnectionFactory();

        _connection = await factory.CreateConnectionAsync();
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_configuration.GetEnvRabbitMqAmqpUri())
        };

        return factory;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}