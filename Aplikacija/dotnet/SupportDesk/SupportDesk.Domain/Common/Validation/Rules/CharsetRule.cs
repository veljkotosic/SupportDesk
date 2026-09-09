using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public class CharsetRule : AbstractRule
{
    private readonly string _stringValue;
    private readonly char[] _charset;

    protected CharsetRule(string stringValue, char[] charset)
    {
        _stringValue = stringValue;
        _charset = charset;
    }

    protected override string ErrorCode => "charset";
    protected override string ErrorMessage => $"Forbidden characters in '{ObjectName}'";

    public override bool Validate()
    {
        foreach (var character in _stringValue)
        {
            if (!_charset.Contains(character))
            {
                return false;
            }
        }
        
        return true;
    }
}