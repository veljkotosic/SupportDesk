using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;

public class TemplateAnswerRepository : Repository<TemplateAnswer>, ITemplateAnswerRepository
{
    public TemplateAnswerRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }
}