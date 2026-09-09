using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;

namespace SupportDesk.Domain.Common.ValueObjects;

public sealed record Email : AbstractValueObject
{
    public string EmailValue { get; init; }

    public Email(string EmailValue)
    {
        this.EmailValue = EmailValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return
        [
            new EmailFormatRule(EmailValue)
        ];
    }
}