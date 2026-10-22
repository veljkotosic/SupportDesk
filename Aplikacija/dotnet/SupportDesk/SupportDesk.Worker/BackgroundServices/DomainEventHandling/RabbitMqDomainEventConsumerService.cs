using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Infrastructure.Messaging.Inbox;
using SupportDesk.Infrastructure.Messaging.Outbox;
using SupportDesk.Infrastructure.Messaging.RabbitMq;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Worker.BackgroundServices.DomainEventHandling;

public sealed class RabbitMqDomainEventConsumerService : BackgroundService
{
    private readonly IRabbitMqConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqDomainEventConsumerService> _logger;

    public RabbitMqDomainEventConsumerService(
        IRabbitMqConnection connection,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqDomainEventConsumerService> logger)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
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
                    exchange: RabbitMqConstants.DomainEventsExchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);
                
                await channel.QueueDeclareAsync(
                    queue: RabbitMqConstants.WorkerDomainEventsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);
                
                await channel.QueueBindAsync(
                    queue: RabbitMqConstants.WorkerDomainEventsQueue,
                    exchange: RabbitMqConstants.DomainEventsExchange,
                    routingKey: "#",
                    cancellationToken: stoppingToken);
                
                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 10,
                    global: false,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (sender, eventArgs) =>
                {
                    OutboxMessage? outboxMessage = null;
                    try
                    {
                        var body = eventArgs.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        outboxMessage = JsonSerializer.Deserialize<OutboxMessage>(json);

                        if (outboxMessage is not null)
                        {
                            await ProcessMessageWithInboxAsync(outboxMessage, stoppingToken);
                        }
                        
                        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        var actualEx = e is System.Reflection.TargetInvocationException tie && tie.InnerException is not null
                            ? tie.InnerException
                            : e;

                        if (actualEx is SupportDesk.Domain.Abstract.Validation.ValidationException)
                        {
                            _logger.LogWarning(actualEx, "Domain event processing failed with validation exception for event {EventId}. Discarding poison message without requeue.", outboxMessage?.Id);
                        }
                        else
                        {
                            _logger.LogError(e, "Error while processing domain event {EventId}. Discarding message without requeue.", outboxMessage?.Id);
                        }

                        await channel.BasicNackAsync(
                            deliveryTag: eventArgs.DeliveryTag,
                            multiple: false,
                            requeue: false,
                            cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: RabbitMqConstants.WorkerDomainEventsQueue,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);
                
                _logger.LogInformation("Worker domain event consumer started listening on queue {Queue}", RabbitMqConstants.WorkerDomainEventsQueue);

                var channelShutdownTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                channel.ChannelShutdownAsync += (sender, args) =>
                {
                    _logger.LogWarning("RabbitMQ channel shut down: {ReplyText}. Worker consumer will reconnect...", args.ReplyText);
                    channelShutdownTcs.TrySetResult(true);
                    return Task.CompletedTask;
                };

                using var reg = stoppingToken.Register(() => channelShutdownTcs.TrySetCanceled());
                await channelShutdownTcs.Task;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker consumer is stopping.");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in Worker domain event consumer. Reconnecting in 5 seconds...");
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

    private async Task ProcessMessageWithInboxAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        
        var eventType = Type.GetType(outboxMessage.EventType)!;
        var domainEvent = JsonSerializer.Deserialize(outboxMessage.Payload, eventType)!;
        
        var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = scope.ServiceProvider.GetServices(handlerInterfaceType);
        
        var userSetter = scope.ServiceProvider.GetRequiredService<IUserContextSetter>();
        userSetter.SetCurrentUserId(outboxMessage.UserId);

        var tenantSetter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        tenantSetter.SetCurrentOrganizationId(outboxMessage.OrganizationId);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var handlerType = handler.GetType();
            var handlerTypeName = handlerType.AssemblyQualifiedName ?? handlerType.FullName!;
            
            var inboxMessage = await dbContext.InboxMessages
                .FirstOrDefaultAsync(m => m.OutboxMessageId == outboxMessage.Id && m.HandlerType == handlerTypeName, cancellationToken);

            if (inboxMessage is not null && inboxMessage.ProcessedOnUtc is not null)
            {
                continue;
            }
            
            if (inboxMessage is null)
            {
                inboxMessage = new InboxMessage
                {
                    OutboxMessageId = outboxMessage.Id,
                    HandlerType = handlerTypeName,
                    Payload = outboxMessage.Payload,
                    UserId = outboxMessage.UserId,
                    OrganizationId = outboxMessage.OrganizationId,
                    ReceivedOnUtc = DateTime.UtcNow,
                    ProcessedOnUtc = null,
                    Error = null
                };
                
                await dbContext.InboxMessages.AddAsync(inboxMessage, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)])!;
                await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;

                inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                inboxMessage.Error = ex.ToString();
                
                await dbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogError(ex, "Domain event handler '{DomainEventHandler}' failed to handle event with id '{DomainEventId}'", handlerTypeName, outboxMessage.Id);
                
                throw; 
            }
        }
    }
}