using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
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
        try
        {
            var channel = await _connection.CreateChannelAsync(stoppingToken);
            
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
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var outboxMessage = JsonSerializer.Deserialize<OutboxMessage>(json);

                    if (outboxMessage is not null)
                    {
                        await ProcessMessageWithInboxAsync(outboxMessage, stoppingToken);
                    }
                    
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, false, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while processing domain event");
                    
                    await channel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMqConstants.WorkerDomainEventsQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);
            
            _logger.LogInformation("Worker domain event consumer started listening on queue {Queue}", RabbitMqConstants.WorkerDomainEventsQueue);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker consumer is stopping.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fatal error in Worker domain event consumer.");
        }
    }

    private async Task ProcessMessageWithInboxAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        
        var inboxMessage = await dbContext.InboxMessages.FindAsync([outboxMessage.Id], cancellationToken);

        if (inboxMessage is not null && inboxMessage.ProcessedOnUtc is not null)
        {
            _logger.LogInformation("Message {OutboxInboxMessageId} already processed by Inbox. Skipping duplicate.", outboxMessage.Id);
            return;
        }

        if (inboxMessage is null)
        {
            inboxMessage = new InboxMessage
            {
                Id = outboxMessage.Id,
                HandlerType = outboxMessage.HandlerType,
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
            var userSetter = scope.ServiceProvider.GetRequiredService<IUserContextSetter>();
            userSetter.SetCurrentUserId(outboxMessage.UserId);

            var tenantSetter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
            tenantSetter.SetCurrentOrganizationId(outboxMessage.OrganizationId);

            var handlerType = Type.GetType(outboxMessage.HandlerType)!;
            var eventType = Type.GetType(outboxMessage.EventType)!;
            var domainEvent = JsonSerializer.Deserialize(outboxMessage.Payload, eventType)!;

            var handler = scope.ServiceProvider.GetRequiredService(handlerType);
            var handleMethod = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)])!;

            await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            
            inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            inboxMessage.Error = e.ToString();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}