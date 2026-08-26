using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer.Options;

namespace SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

public sealed record TemplateAnswerText : AbstractValueObject
{
    public string TextValue { get; init; }
    
    public TemplateAnswerText(string TextValue)
    {
        this.TextValue = TextValue;
        ValidateValueObject(); 
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(TextValue, TemplateAnswerOptionsDefaults.TextMinimumLength),
            new MaxLengthRule(TextValue, TemplateAnswerOptionsDefaults.TextMaximumLength)
        ];
    }
}