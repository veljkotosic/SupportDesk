using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Faq.Repository;

public class FaqRepository : Repository<Faq>, IFaqRepository
{
    public FaqRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }
}