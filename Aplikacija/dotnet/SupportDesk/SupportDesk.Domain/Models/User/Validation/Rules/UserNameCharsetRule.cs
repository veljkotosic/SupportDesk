using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Utility.Text;

namespace SupportDesk.Domain.Models.User.Validation.Rules;

public sealed class UsernameCharsetRule : CharsetRule, IRuleMetadata
{
    public UsernameCharsetRule(string stringValue) 
        : base(stringValue, GetUsernameCharset())
    {
        
    }

    private static char[] GetUsernameCharset()
    {
        var alphanumericCharset = Charset.GetAlphanumericCharset();

        char[] usernameCharset = [..alphanumericCharset, '_'];

        return usernameCharset;
    }

    public static string ErrorCodeString => "username_charset";
    protected override string ErrorCode => ErrorCodeString;

    protected override string ErrorMessage =>
        "Username contains invalid characters, only alphanumeric characters and '_' are allowed";
}