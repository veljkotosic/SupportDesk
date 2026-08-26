using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

public sealed record SupportAgentInviteCode : AbstractValueObject
{
    public Guid CodeValue { get; init; }
    
    public SupportAgentInviteCode(Guid CodeValue)
    {
        this.CodeValue = CodeValue;
    }
}