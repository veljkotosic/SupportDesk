using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Faq.Options;

namespace SupportDesk.Domain.Models.Faq.ValueObjects;

public sealed record FaqQuestion : AbstractValueObject
{
    public string QuestionValue { get; init; }
    
    public FaqQuestion(string QuestionValue)
    {
        this.QuestionValue = QuestionValue;
        ValidateValueObject();   
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(QuestionValue, FaqOptionsDefaults.QuestionMinimumLength),
            new MaxLengthRule(QuestionValue, FaqOptionsDefaults.QuestionMaximumLength)
        ];
    }
}