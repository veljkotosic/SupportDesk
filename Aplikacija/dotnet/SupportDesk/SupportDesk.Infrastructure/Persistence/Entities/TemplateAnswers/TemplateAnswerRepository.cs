using SupportDesk.Application.Abstract.Event;
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
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}