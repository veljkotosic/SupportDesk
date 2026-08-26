using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteCannotBeUsedBeforeItsCreatedRule : AbstractRule, IRuleMetadata
{
    private readonly CreatedAt _createdAt;
    private readonly SupportAgentInviteUsedAt? _supportAgentInviteUsedAt;
    
    public SupportAgentInviteCannotBeUsedBeforeItsCreatedRule(
        CreatedAt createdAt, 
        SupportAgentInviteUsedAt? supportAgentInviteUsedAt)
    {
        _createdAt = createdAt;
        _supportAgentInviteUsedAt = supportAgentInviteUsedAt;
    }
    
    public static string ErrorCodeString => "support_agent_invite_cannot_be_used_before_its_created";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Support agent invite cannot be used before it is created";
    public override bool Validate()
    {
        if (_supportAgentInviteUsedAt is null)
        {
            return true;
        }
        
        return _supportAgentInviteUsedAt.UsedAtValue > _createdAt.CreatedAtValue;
    }

}