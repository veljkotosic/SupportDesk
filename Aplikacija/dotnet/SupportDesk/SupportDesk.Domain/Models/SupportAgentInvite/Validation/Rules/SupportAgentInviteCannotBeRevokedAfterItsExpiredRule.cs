using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteCannotBeRevokedAfterItsExpiredRule : AbstractRule, IRuleMetadata
{
    private readonly SupportAgentInviteExpiresAt _supportAgentInviteExpiresAt;
    private readonly SupportAgentInviteRevokedAt? _supportAgentInviteRevokedAt;

    public SupportAgentInviteCannotBeRevokedAfterItsExpiredRule(
        SupportAgentInviteExpiresAt supportAgentInviteExpiresAt,
        SupportAgentInviteRevokedAt? supportAgentInviteRevokedAt)
    {
        _supportAgentInviteExpiresAt = supportAgentInviteExpiresAt;
        _supportAgentInviteRevokedAt = supportAgentInviteRevokedAt;
    }
    
    public static string ErrorCodeString => "support_agent_invite_cannot_be_revoked_after_its_expired";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Support agent invite cannot be revoked after it is expired";
    public override bool Validate()
    {
        if (_supportAgentInviteRevokedAt is null)
        {
            return true;
        }
        
        return _supportAgentInviteExpiresAt.ExpiresAtValue > _supportAgentInviteRevokedAt.RevokedAtValue;
    }

}