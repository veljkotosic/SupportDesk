using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Message.Repository;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }
}