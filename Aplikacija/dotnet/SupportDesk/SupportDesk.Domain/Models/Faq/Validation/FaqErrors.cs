using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Faq.ValueObjects;

namespace SupportDesk.Domain.Models.Faq.Validation;

public sealed class FaqErrors : AbstractErrors<Faq, FaqId>
{
    public static ValidationError AlreadyDeleted(FaqId faqId)
    {
        string code = "already_deleted";
        string message = $"Faq '{faqId}' already deleted.";
        
        return CreateValidationError(code, message);
    }
}