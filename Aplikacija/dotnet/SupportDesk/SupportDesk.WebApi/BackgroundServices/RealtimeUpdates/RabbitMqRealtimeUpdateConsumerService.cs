using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Infrastructure.Messaging.RabbitMq;
using SupportDesk.Infrastructure.Messaging.Realtime;
using SupportDesk.WebApi.Hubs;

namespace SupportDesk.WebApi.BackgroundServices.RealtimeUpdates;

public class RabbitMqRealtimeUpdateConsumerService : BackgroundService
{
    private readonly IRabbitMqConnection _connection;
    private readonly IHubContext<CustomerDashboardHub> _customerDashboardHub;
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHub;
    private readonly IHubContext<TicketHub> _ticketHub;
    private readonly ILogger<RabbitMqRealtimeUpdateConsumerService> _logger;

    public RabbitMqRealtimeUpdateConsumerService(
        IRabbitMqConnection connection,
        IHubContext<CustomerDashboardHub> customerDashboardHub,
        IHubContext<OrganizationDashboardHub> organizationDashboardHub,
        IHubContext<TicketHub> ticketHub,
        ILogger<RabbitMqRealtimeUpdateConsumerService> logger)
    {
        _connection = connection;
        _customerDashboardHub = customerDashboardHub;
        _organizationDashboardHub = organizationDashboardHub;
        _ticketHub = ticketHub;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var channel = await _connection.CreateChannelAsync(stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: RabbitMqConstants.RealtimeUpdatesExchange,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                var queueDeclareResult = await channel.QueueDeclareAsync(
                    queue: string.Empty,
                    durable: false,
                    exclusive: true,
                    autoDelete: true,
                    cancellationToken: stoppingToken);

                var queueName = queueDeclareResult.QueueName;

                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: RabbitMqConstants.RealtimeUpdatesExchange,
                    routingKey: string.Empty,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<RealtimeUpdateMessage>(json);

                        if (message is not null)
                        {
                            await DispatchToHubAsync(message, stoppingToken);
                        }

                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to dispatch real-time update message to SignalR hub.");
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Web API real-time update consumer started listening on queue {QueueName}", queueName);

                var channelShutdownTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                channel.ChannelShutdownAsync += (sender, args) =>
                {
                    _logger.LogWarning("RabbitMQ channel shut down: {ReplyText}. Real-time consumer will reconnect...", args.ReplyText);
                    channelShutdownTcs.TrySetResult(true);
                    return Task.CompletedTask;
                };

                using var reg = stoppingToken.Register(() => channelShutdownTcs.TrySetCanceled());
                await channelShutdownTcs.Task;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Web API real-time update consumer is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Web API real-time update consumer. Reconnecting in 5 seconds...");
                try
                {
                    await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
    
    private async Task DispatchToHubAsync(RealtimeUpdateMessage message, CancellationToken cancellationToken)
    {
        var clientProxy = message.HubType switch
        {
            RealtimeHubType.CustomerDashboard => _customerDashboardHub.Clients.Group(message.TargetGroup),
            RealtimeHubType.OrganizationDashboard => _organizationDashboardHub.Clients.Group(message.TargetGroup),
            RealtimeHubType.Ticket => _ticketHub.Clients.Group(message.TargetGroup),
            _ => throw new ArgumentOutOfRangeException(nameof(message.HubType), $"Unknown hub type {message.HubType}")
        };
        
        using var document = JsonDocument.Parse(message.PayloadJson);
        var payloadObject = document.RootElement.Clone();

        await clientProxy.SendAsync(message.Action, payloadObject, cancellationToken);
    }
}