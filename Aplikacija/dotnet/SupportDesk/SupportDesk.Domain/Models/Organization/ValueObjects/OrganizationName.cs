using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Organization.Options;
using SupportDesk.Domain.Models.Organization.Validation.Rules;

namespace SupportDesk.Domain.Models.Organization.ValueObjects;

public sealed record OrganizationName : AbstractValueObject
{
    public string NameValue { get; init; }
    
    public OrganizationName(string NameValue)
    {
        this.NameValue = NameValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [
            new MinLengthRule(NameValue, OrganizationOptionsDefaults.NameMinimumLength),
            new MaxLengthRule(NameValue, OrganizationOptionsDefaults.NameMaximumLength),
            new OrganizationNameCharsetRule(NameValue)
        ];
    }
    
    public static implicit operator string(OrganizationName name) => name.NameValue;  
}