using System.Numerics;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Common.Validation.Rules;

public sealed class MinValueRule<T> : AbstractRule, IRuleMetadata
    where T : INumber<T>
{
    private readonly T _value;
    private readonly T _minimum;

    public MinValueRule(T value, T minimum)
    {
        _value = value;
        _minimum = minimum;
    }

    public static string ErrorCodeString => "min_value";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => $"Value of '{ObjectName}' should be at least {_minimum}";
    
    public override bool Validate()
    {
        return _value >= _minimum;
    }
}