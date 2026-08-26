using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.TemplateAnswer.Options;

namespace SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;

public sealed record TemplateAnswerTitle : AbstractValueObject
{
    public string TitleValue { get; init; }

    public TemplateAnswerTitle(string titleValue)
    {
        this.TitleValue = titleValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(TitleValue, TemplateAnswerOptionsDefaults.TitleMinimumLength),
            new MaxLengthRule(TitleValue, TemplateAnswerOptionsDefaults.TitleMaximumLength)
        ];
    }
}