using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class MinLengthRule : AbstractRule, IRuleMetadata
{
    private readonly string _stringValue;
    private readonly int _minimumLength;
    
    public MinLengthRule(string stringValue, int minimumLength)
    {
        _stringValue = stringValue;
        _minimumLength = minimumLength;
    }
    
    public static string ErrorCodeString => "minimum_length";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage 
        => $"Length of '{ObjectName}' must be at least {_minimumLength} characters";
    
    public override bool Validate()
    {
        return _stringValue.Length >= _minimumLength;
    }
}