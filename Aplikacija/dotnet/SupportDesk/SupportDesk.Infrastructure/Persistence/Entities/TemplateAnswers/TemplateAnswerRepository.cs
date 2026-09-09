using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Repository;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.TemplateAnswers;

public sealed class TemplateAnswerRepository
    : AbstractRepository<TemplateAnswer, TemplateAnswerId>, ITemplateAnswerRepository
{
    public TemplateAnswerRepository(
        SupportDeskDbContext context,
        IServiceProvider serviceProvider,
        IUserContext userContext,
        ITenantContext tenantContext)
        : base(context, serviceProvider, userContext, tenantContext)
    {
        
    }
}