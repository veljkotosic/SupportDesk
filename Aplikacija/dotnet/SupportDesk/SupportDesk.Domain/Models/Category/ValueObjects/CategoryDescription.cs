using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Category.Validation.Rules;

namespace SupportDesk.Domain.Models.Category.ValueObjects;

public sealed record CategoryDescription : AbstractValueObject
{
    public string DescriptionValue { get; init; }
    
    public CategoryDescription(string DescriptionValue)
    {
        this.DescriptionValue = DescriptionValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MaxLengthRule(DescriptionValue, CategoryOptionsDefaults.DescriptionMaximumLength),
            new CategoryDescriptionCharsetRule(DescriptionValue)
        ];
    }
}