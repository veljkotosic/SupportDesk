using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteCannotBeRevokedBeforeItsCreatedRule : AbstractRule, IRuleMetadata
{
    private readonly CreatedAt _createdAt;
    private readonly SupportAgentInviteRevokedAt? _supportAgentInviteRevokedAt;
    
    public SupportAgentInviteCannotBeRevokedBeforeItsCreatedRule(
        CreatedAt createdAt, 
        SupportAgentInviteRevokedAt? supportAgentInviteRevokedAt)
    {
        _createdAt = createdAt;
        _supportAgentInviteRevokedAt = supportAgentInviteRevokedAt;
    }
    
    public static string ErrorCodeString => "support_agent_invite_cannot_be_revoked_before_its_created";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Support agent invite cannot be revoked before it is created";
    public override bool Validate()
    {
        if (_supportAgentInviteRevokedAt is null)
        {
            return true;
        }
        
        return _supportAgentInviteRevokedAt.RevokedAtValue > _createdAt.CreatedAtValue;
    }
}