using System.Numerics;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class MaxValueRule<T> : AbstractRule, IRuleMetadata
    where T : INumber<T>
{
    private readonly T _value;
    private readonly T _maximum;

    public MaxValueRule(T value, T maximum)
    {
        _value = value;
        _maximum = maximum;
    }
    
    public static string ErrorCodeString => "max_value";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => $"Value of '{ObjectName}' should be at most {_maximum}";
    
    public override bool Validate()
    {
        return _value <= _maximum;
    }
}