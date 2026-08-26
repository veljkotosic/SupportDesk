using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Category.Validation.Rules;

namespace SupportDesk.Domain.Models.Category.ValueObjects;

public sealed record CategoryName : AbstractValueObject
{
    public string NameValue { get; init; }
    
    public CategoryName(string NameValue)
    {
        this.NameValue = NameValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(NameValue, CategoryOptionsDefaults.NameMinimumLength),
            new MaxLengthRule(NameValue, CategoryOptionsDefaults.NameMaximumLength),
            new CategoryNameCharsetRule(NameValue)
        ];
    }
}