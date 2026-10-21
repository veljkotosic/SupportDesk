using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;

namespace SupportDesk.Infrastructure.Messaging.RabbitMq;

internal sealed class RabbitMqConnection : IRabbitMqConnection, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(IConfiguration configuration, ILogger<RabbitMqConnection> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await EnsureConnectedAsync(cancellationToken);

        try
        {
            return await _connection!.CreateChannelAsync(null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create RabbitMQ channel on existing connection. Invalidating connection and retrying...");
            
            await InvalidateConnectionAsync();
            await EnsureConnectedAsync(cancellationToken);
            
            return await _connection!.CreateChannelAsync(null, cancellationToken);
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        
        try
        {
            if (_connection is { IsOpen: true })
            {
                return;
            }

            if (_connection is not null)
            {
                try
                {
                    await _connection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error disposing stale RabbitMQ connection.");
                }
                
                _connection = null;
            }

            var factory = CreateConnectionFactory();
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            
            _logger.LogInformation("Successfully connected to RabbitMQ broker.");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task InvalidateConnectionAsync()
    {
        await _connectionLock.WaitAsync();
        
        try
        {
            if (_connection is not null)
            {
                try
                {
                    await _connection.DisposeAsync();
                }
                catch
                {
                    // Ignored
                }

                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private ConnectionFactory CreateConnectionFactory()
    {
        return new ConnectionFactory
        {
            Uri = new Uri(_configuration.GetEnvRabbitMqAmqpUri()),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection is not null)
        {
            try
            {
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error while disposing RabbitMQ connection.");
            }
        }

        _connectionLock.Dispose();
    }
}