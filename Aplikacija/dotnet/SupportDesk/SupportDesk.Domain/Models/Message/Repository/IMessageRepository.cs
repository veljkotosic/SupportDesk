using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Message.ValueObjects;

namespace SupportDesk.Domain.Models.Message.Repository;

public interface IMessageRepository : IAbstractRepository<Message, MessageId>
{
    
}