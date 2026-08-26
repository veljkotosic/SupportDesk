using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Faq.Events;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Faq;

public sealed class Faq : AbstractDomainModel<FaqId>
{
    public OrganizationId OrganizationId { get; private set; }
    public FaqQuestion Question { get; private set; }
    public FaqAnswer Answer { get; private set; }
    
    private Faq(
        FaqId id,
        OrganizationId organizationId,
        FaqQuestion question,
        FaqAnswer answer
    ) : base(id)
    {
        OrganizationId = organizationId;
        Question = question;
        Answer = answer;
    }

    public static Faq Create(Guid organizationId, string question, string answer)
    {
        var idVo = FaqId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var questionVo = new FaqQuestion(question);
        var answerVo = new FaqAnswer(answer);

        var createdFaq = new Faq(idVo, organizationIdVo, questionVo, answerVo);
        
        createdFaq.RaiseDomainEvent(new FaqCreatedDomainEvent(createdFaq.Id.IdValue));
        
        return createdFaq;       
    }
}