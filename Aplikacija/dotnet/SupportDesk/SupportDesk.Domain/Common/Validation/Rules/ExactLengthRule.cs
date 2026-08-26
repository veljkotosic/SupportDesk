using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class ExactLength : AbstractRule, IRuleMetadata
{
    private readonly string _stringValue;
    private readonly int _exactLength;
    
    public ExactLength(string stringValue, int exactLength)
    {
        _stringValue = stringValue;
        _exactLength = exactLength;
    }
    
    public static string ErrorCodeString => "exact_length";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage 
        => $"Length of '{ObjectName}' must be {_exactLength} characters";
    
    public override bool Validate()
    {
        return _stringValue.Length == _exactLength;
    }
}