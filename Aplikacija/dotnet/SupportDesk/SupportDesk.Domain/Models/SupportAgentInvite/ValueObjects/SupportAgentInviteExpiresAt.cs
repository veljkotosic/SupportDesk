using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;

namespace SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

public sealed record SupportAgentInviteExpiresAt : AbstractValueObject
{
    public DateTime ExpiresAtValue { get; init; }
    
    public SupportAgentInviteExpiresAt(DateTime ExpiresAtValue)
    {
        this.ExpiresAtValue = ExpiresAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}