using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Message.Repository;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Messages;

public sealed class MessageRepository
    : AbstractRepository<Message, MessageId>, IMessageRepository
{
    public MessageRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}