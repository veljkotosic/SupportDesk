using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Models.User.Validation.Rules;

public sealed class UserCanOnlyLogoutHimselfRule : AbstractRule, IRuleMetadata
{
    private readonly Guid _userThatLogsOutId;
    private readonly Guid _userToLogOutId;
    
    public UserCanOnlyLogoutHimselfRule(Guid userThatLogsOutId, Guid userToLogOutId)
    {
        _userThatLogsOutId = userThatLogsOutId;
        _userToLogOutId = userToLogOutId;
    }
    
    public static string ErrorCodeString => "user_can_only_logout_himself";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "User can only logout himself";
    public override bool Validate()
    {
        return _userThatLogsOutId == _userToLogOutId;
    }

}