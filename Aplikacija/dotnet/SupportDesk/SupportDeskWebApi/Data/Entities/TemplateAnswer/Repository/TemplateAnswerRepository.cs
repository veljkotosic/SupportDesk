using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;

public class TemplateAnswerRepository : Repository<TemplateAnswer>, ITemplateAnswerRepository
{
    public TemplateAnswerRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task DeleteAsync(TemplateAnswer entity, CancellationToken cancellationToken = default)
    {
        await Context.TemplateAnswers
            .Where(ta => ta.Id == entity.Id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<TemplateAnswer?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await Context.TemplateAnswers.FirstOrDefaultAsync(ta => ta.Title == title, cancellationToken);
    }
}