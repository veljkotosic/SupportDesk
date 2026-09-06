using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation;

public sealed class SupportAgentInviteErrors : AbstractErrors<SupportAgentInvite, SupportAgentInviteId>
{
    public static ValidationError InvalidInviteCode(SupportAgentInviteCode inviteCode)
    {
        const string errorCode = "invalid_invite_code";
        return new ValidationError(errorCode, $"Invalid invite code '{inviteCode.CodeValue}' cannot be used.");
    }
    
    public static ValidationError ExpiredInviteCode(SupportAgentInviteCode inviteCode)
    {
        const string errorCode = "expired_invite_code";
        return new ValidationError(errorCode, $"Expired invite code '{inviteCode.CodeValue}' cannot be used.");
    }

    public static ValidationError CannotRevoke()
    {
        const string errorCode = "cannot_revoke_invite";
        return new ValidationError(errorCode, $"Invite code has already been used or revoked.");   
    }
}