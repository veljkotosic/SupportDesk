using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SupportDesk.Infrastructure.Messaging;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.OutboxProcessor.BackgroundServices.OutboxProcessing;

public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        IDomainEventPublisher domainEventPublisher,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _domainEventPublisher = domainEventPublisher;
        _logger = logger;
    }
    
    private const int BatchSize = 100;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();

                var messages = await dbContext.OutboxMessages
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (messages.Count > 0)
                {
                    foreach (var message in messages)
                    {
                        try
                        {
                            await _domainEventPublisher.PublishAsync(message, stoppingToken);
                            message.ProcessedOnUtc = DateTime.UtcNow;
                        }
                        catch (Exception e)
                        {
                            message.Error = e.ToString();
                            _logger.LogError(e, "Error processing outbox message with id {MessageId}", message.Id);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                else
                {
                    await Task.Delay(250, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing outbox message batch");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}