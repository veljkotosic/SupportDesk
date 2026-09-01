using SupportDesk.Infrastructure.Messaging.Outbox;

namespace SupportDesk.Infrastructure.Messaging;

public interface IDomainEventPublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}