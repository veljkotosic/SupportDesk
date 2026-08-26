using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class MaxLengthRule : AbstractRule, IRuleMetadata
{
    private readonly string _stringValue;
    private readonly int _maximumLength;
    
    public MaxLengthRule(string stringValue, int maximumLength)
    {
        _stringValue = stringValue;
        _maximumLength = maximumLength;
    }
    
    public static string ErrorCodeString => "maximum_length";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage 
        => $"Length of '{ObjectName}' must be at most {_maximumLength} characters";
    
    public override bool Validate()
    {
        return _stringValue.Length <= _maximumLength;
    }
}