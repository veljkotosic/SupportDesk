using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.TemplateAnswer.Events;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer;

public sealed class TemplateAnswer : AbstractDomainModel<TemplateAnswerId>
{
    public OrganizationId OrganizationId { get; private set; }
    public TemplateAnswerTitle Title { get; private set; }
    public TemplateAnswerText Text { get; private set; }
    
    private TemplateAnswer(
        TemplateAnswerId id,
        OrganizationId organizationId,
        TemplateAnswerTitle title,
        TemplateAnswerText text
    ) : base(id)
    {
        OrganizationId = organizationId;
        Title = title;
        Text = text;
    }

    public static TemplateAnswer Create(Guid organizationId, string title, string text)
    {
        var idVo = TemplateAnswerId.NewId();
        var organizationIdVo = new OrganizationId(organizationId);
        var titleVo = new TemplateAnswerTitle(title);
        var textVo = new TemplateAnswerText(text);
        
        var createdTemplateAnswer = new TemplateAnswer(idVo, organizationIdVo, titleVo, textVo);
        
        createdTemplateAnswer.RaiseDomainEvent(new TemplateAnswerCreatedDomainEvent(createdTemplateAnswer.Id.IdValue));
        
        return createdTemplateAnswer;
    }
}