using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

public sealed record SupportAgentInviteUsedAt : AbstractValueObject
{
    public DateTime UsedAtValue { get; init; }
    
    public SupportAgentInviteUsedAt(DateTime UsedAtValue)
    {
        this.UsedAtValue = UsedAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}