using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Models.User.Validation.Rules;

public sealed class UserCanOnlyRefreshHisLoginRule : AbstractRule, IRuleMetadata
{
    private readonly Guid _userThatRefreshesLoginId;
    private readonly Guid _userToRefreshLoginId;

    public UserCanOnlyRefreshHisLoginRule(Guid userThatRefreshesLoginId, Guid userToRefreshLoginId)
    {
        _userThatRefreshesLoginId = userThatRefreshesLoginId;
        _userToRefreshLoginId = userToRefreshLoginId;
    }
    
    public static string ErrorCodeString => "user_can_only_refresh_his_login";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "User can only refresh his login";
    public override bool Validate()
    {
        return _userThatRefreshesLoginId == _userToRefreshLoginId;
    }

}