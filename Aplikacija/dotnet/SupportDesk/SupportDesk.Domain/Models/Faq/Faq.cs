using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Faq.Events;
using SupportDesk.Domain.Models.Faq.Validation;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Faq;

public sealed class Faq : AbstractDomainModel<FaqId>
{
    public OrganizationId OrganizationId { get; private set; }
    public FaqQuestion Question { get; private set; }
    public FaqAnswer Answer { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public DeletedAt? DeletedAt { get; private set; }

    internal Faq()
    {
        
    }
    
    private Faq(
        FaqId id,
        OrganizationId organizationId,
        FaqQuestion question,
        FaqAnswer answer,
        CreatedAt createdAt,
        DeletedAt? deletedAt = null
    ) : base(id)
    {
        OrganizationId = organizationId;
        Question = question;
        Answer = answer;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;       
    }

    public static Faq Create(Guid organizationId, string question, string answer, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;    
        
        var idVo = FaqId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var questionVo = new FaqQuestion(question);
        var answerVo = new FaqAnswer(answer);
        var createdAtVo = new CreatedAt(now);      

        var createdFaq = new Faq(idVo, organizationIdVo, questionVo, answerVo, createdAtVo);
        
        createdFaq.RaiseDomainEvent(new FaqCreatedDomainEvent(createdFaq.Id));
        
        return createdFaq;       
    }

    public void Delete(TimeProvider timeProvider)
    {
        if (DeletedAt is not null)
        {
            throw new ValidationException(FaqErrors.AlreadyDeleted(Id));
        }
        
        var now = timeProvider.GetUtcNow().UtcDateTime;
        
        DeletedAt = new DeletedAt(now);      
        
        RaiseDomainEvent(new FaqDeletedDomainEvent(Id));      
    }

    public void UpdateDetails(string? question, string? answer)
    {
        var hasChanged = false;

        if (question is not null)
        {
            var newQuestion = new FaqQuestion(question);
            if (newQuestion != Question)
            {
                Question = newQuestion;
                hasChanged = true;
            }
        }
        
        if (answer is not null)
        {
            var newAnswer = new FaqAnswer(answer);
            if (newAnswer != Answer)
            {
                Answer = newAnswer;
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            RaiseDomainEvent(new FaqDetailsUpdatedDomainEvent(Id, Question, Answer));       
        }
    }
}