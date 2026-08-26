using System.Net.Mail;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public class EmailFormatRule : AbstractRule, IRuleMetadata
{
    private readonly string _emailValue;
    
    public EmailFormatRule(string emailValue)
    {
        _emailValue = emailValue;
    }
    
    public static string ErrorCodeString => "email_invalid";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Email is not in the valid format";
    
    public override bool Validate()
    {
        try
        {
            _ = new MailAddress(_emailValue);
        }
        catch
        {
            return false;
        }

        return true;
    }
}