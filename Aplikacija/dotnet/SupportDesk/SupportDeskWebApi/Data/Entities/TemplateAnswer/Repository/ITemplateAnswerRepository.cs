using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;

public interface ITemplateAnswerRepository 
    : IRepository<TemplateAnswer>, IDeleteRepository<TemplateAnswer>
{
    Task<TemplateAnswer?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
}