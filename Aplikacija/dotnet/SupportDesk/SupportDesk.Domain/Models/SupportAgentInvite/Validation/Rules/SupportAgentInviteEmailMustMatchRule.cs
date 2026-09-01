using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteEmailMustMatchRule : AbstractRule, IRuleMetadata
{
    private readonly SupportAgentInvite? _supportAgentInvite;
    private readonly Email _email;
    
    public SupportAgentInviteEmailMustMatchRule(
        SupportAgentInvite? supportAgentInvite, 
        Email email)
    {
        _supportAgentInvite = supportAgentInvite;
        _email = email;
    }
    
    public static string ErrorCodeString => "invalid_code";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Invalid invite code";
    public override bool Validate()
    {
        if (_supportAgentInvite is null)
        {
            return false;
        }

        return _supportAgentInvite.Email == _email;
    }

}