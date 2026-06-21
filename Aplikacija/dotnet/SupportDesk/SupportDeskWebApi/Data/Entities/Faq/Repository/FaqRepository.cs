using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Faq.Repository;

public class FaqRepository : Repository<Faq>, IFaqRepository
{
    public FaqRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task DeleteAsync(Faq entity, CancellationToken cancellationToken = default)
    {
        await Context.Faqs
            .Where(f => f.Id == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}