using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.SupportAgentInvite.Enums;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteStatusMustMatchTimestampsRule : AbstractRule, IRuleMetadata
{
    private readonly SupportAgentInviteStatus _supportAgentInviteStatus;
    private readonly SupportAgentInviteUsedAt? _supportAgentInviteUsedAt;
    private readonly SupportAgentInviteRevokedAt? _supportAgentInviteRevokedAt;

    public SupportAgentInviteStatusMustMatchTimestampsRule(
        SupportAgentInviteStatus supportAgentInviteStatus,
        SupportAgentInviteUsedAt? supportAgentInviteUsedAt,
        SupportAgentInviteRevokedAt? supportAgentInviteRevokedAt)
    {
        _supportAgentInviteStatus = supportAgentInviteStatus;
        _supportAgentInviteUsedAt = supportAgentInviteUsedAt;
        _supportAgentInviteRevokedAt = supportAgentInviteRevokedAt;
    }
    
    public static string ErrorCodeString => "support_agent_invite_status_must_match_timestamps";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Support Agent Invite status must match timestamps";
    public override bool Validate()
    {
        return _supportAgentInviteStatus switch
        {
            SupportAgentInviteStatus.Revoked => _supportAgentInviteRevokedAt is not null && _supportAgentInviteUsedAt is null,
            SupportAgentInviteStatus.Used => _supportAgentInviteUsedAt is not null && _supportAgentInviteRevokedAt is null,
            SupportAgentInviteStatus.Active => _supportAgentInviteUsedAt is null && _supportAgentInviteRevokedAt is null,
            _ => true
        };
    }

}