using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer.Repository;

public interface ITemplateAnswerRepository : IAbstractRepository<TemplateAnswer, TemplateAnswerId>
{
    
}