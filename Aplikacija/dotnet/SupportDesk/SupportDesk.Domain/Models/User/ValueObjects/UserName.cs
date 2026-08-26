using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.User.ValueObjects;

public sealed record UserName : AbstractValueObject
{
    public string UserNameValue { get; init; }
    
    public UserName(string UserNameValue)
    {
        this.UserNameValue = UserNameValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}