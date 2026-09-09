using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq.Options;

namespace SupportDesk.Domain.Models.Faq.ValueObjects;

public sealed record FaqAnswer : AbstractValueObject
{
    public string AnswerValue { get; init; }
    
    public FaqAnswer(string AnswerValue)
    {
        this.AnswerValue = AnswerValue;
        ValidateValueObject();  
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(AnswerValue, FaqOptionsDefaults.AnswerMinimumLength),
            new MaxLengthRule(AnswerValue, FaqOptionsDefaults.AnswerMaximumLength)
        ];
    }
}