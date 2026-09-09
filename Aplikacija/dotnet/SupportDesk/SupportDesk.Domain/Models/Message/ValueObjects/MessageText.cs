using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Message.Options;

namespace SupportDesk.Domain.Models.Message.ValueObjects;

public sealed record MessageText : AbstractValueObject
{
    public string TextValue { get; init; }
    
    public MessageText(string TextValue)
    {
        this.TextValue = TextValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(TextValue, MessageOptionsDefaults.TextMinimumLength),
            new MaxLengthRule(TextValue, MessageOptionsDefaults.TextMaximumLength)
        ];
    }
}