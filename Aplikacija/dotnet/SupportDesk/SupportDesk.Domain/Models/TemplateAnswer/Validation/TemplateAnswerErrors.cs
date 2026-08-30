using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

namespace SupportDesk.Domain.Models.TemplateAnswer.Validation;

public sealed class TemplateAnswerErrors : AbstractErrors<TemplateAnswer, TemplateAnswerId>
{
    public static ValidationError AlreadyDeleted(TemplateAnswerId templateAnswerId)
    {
        string code = "already_deleted";
        string message = $"Template answer with id '{templateAnswerId.IdValue}' already deleted.";
        
        return CreateValidationError(code, message);
    }
}