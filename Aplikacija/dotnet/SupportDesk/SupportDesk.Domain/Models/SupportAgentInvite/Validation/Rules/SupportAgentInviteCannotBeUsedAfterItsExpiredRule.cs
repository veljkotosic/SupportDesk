using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteCannotBeUsedAfterItsExpiredRule : AbstractRule, IRuleMetadata
{
    private readonly SupportAgentInviteExpiresAt _supportAgentInviteExpiresAt;
    private readonly SupportAgentInviteUsedAt? _supportAgentInviteUsedAt;

    public SupportAgentInviteCannotBeUsedAfterItsExpiredRule(
        SupportAgentInviteExpiresAt supportAgentInviteExpiresAt,
        SupportAgentInviteUsedAt? supportAgentInviteUsedAt)
    {
        _supportAgentInviteExpiresAt = supportAgentInviteExpiresAt;
        _supportAgentInviteUsedAt = supportAgentInviteUsedAt;
    }
    
    public static string ErrorCodeString => "support_agent_invite_cannot_be_used_after_its_expired";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Support agent invite cannot be used after it is expired";
    public override bool Validate()
    {
        if (_supportAgentInviteUsedAt is null)
        {
            return true;
        }
        
        return _supportAgentInviteExpiresAt.ExpiresAtValue > _supportAgentInviteUsedAt.UsedAtValue;
    }

}