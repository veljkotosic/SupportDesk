using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using SupportDesk.Domain.Models.TemplateAnswer.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer;

public sealed class TemplateAnswer : AbstractDomainModel<TemplateAnswerId>
{
    public OrganizationId OrganizationId { get; private set; }
    public TemplateAnswerTitle Title { get; private set; }
    public TemplateAnswerText Text { get; private set; }
    public CreatedAt CreatedAt { get; private set; }
    public DeletedAt? DeletedAt { get; private set; }

    internal TemplateAnswer()
    {
        
    }
    
    private TemplateAnswer(
        TemplateAnswerId id,
        OrganizationId organizationId,
        TemplateAnswerTitle title,
        TemplateAnswerText text, 
        CreatedAt createdAt,
        DeletedAt? deletedAt = null)
        : base(id)
    {
        OrganizationId = organizationId;
        Title = title;
        Text = text;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;       
    }

    public static TemplateAnswer Create(Guid organizationId, string title, string text, TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;    
        
        var idVo = TemplateAnswerId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var titleVo = new TemplateAnswerTitle(title);
        var textVo = new TemplateAnswerText(text);
        var createdAtVo = new CreatedAt(now);     
        
        var createdTemplateAnswer = new TemplateAnswer(idVo, organizationIdVo, titleVo, textVo, createdAtVo);
        
        createdTemplateAnswer.RaiseDomainEvent(new TemplateAnswerCreatedDomainEvent(createdTemplateAnswer.Id));
        
        return createdTemplateAnswer;
    }

    public void Delete(TimeProvider timeProvider)
    {
        if (DeletedAt is not null)
        {
            throw new ValidationException(TemplateAnswerErrors.AlreadyDeleted(Id));
        }
        
        var now = timeProvider.GetUtcNow().UtcDateTime;

        DeletedAt = new DeletedAt(now);
        
        RaiseDomainEvent(new TemplateAnswerDeletedDomainEvent(Id));
    }

    public void UpdateDetails(string? title, string? text)
    {
        var hasChanged = false;
        
        if (title is not null)
        {
            var newTitle = new TemplateAnswerTitle(title);
            if (newTitle != Title)
            {
                Title = newTitle;
                hasChanged = true;
            }
        }
        
        if (text is not null)
        {
            var newText = new TemplateAnswerText(text);
            if (newText != Text)
            {
                Text = newText;
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            RaiseDomainEvent(new TemplateAnswerDetailsUpdatedDomainEvent(Id, Title, Text));      
        }
    }
}