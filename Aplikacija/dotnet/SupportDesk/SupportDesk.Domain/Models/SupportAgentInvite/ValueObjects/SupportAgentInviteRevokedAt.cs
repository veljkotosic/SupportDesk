using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

public sealed record SupportAgentInviteRevokedAt : AbstractValueObject
{
    public DateTime RevokedAtValue { get; init; }
    
    public SupportAgentInviteRevokedAt(DateTime RevokedAtValue)
    {
        this.RevokedAtValue = RevokedAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}