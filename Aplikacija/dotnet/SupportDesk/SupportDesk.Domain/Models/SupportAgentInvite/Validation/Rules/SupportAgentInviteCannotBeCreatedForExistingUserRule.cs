using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;

public sealed class SupportAgentInviteCannotBeCreatedForExistingUserRule : AbstractRule, IRuleMetadata
{
    private readonly User.User? _user;

    public SupportAgentInviteCannotBeCreatedForExistingUserRule(User.User? user)
    {
        _user = user;
    }

    public static string ErrorCodeString => "user_already_registered";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "User with this email already registered.";
    public override bool Validate()
    {
        return _user is null;
    }

}