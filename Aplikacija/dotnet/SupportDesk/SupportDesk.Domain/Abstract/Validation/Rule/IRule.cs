namespace SupportDesk.Domain.Abstract.Validation.Rule;

/// <summary>
/// Defines a single validation rule capable of checking a condition and producing a validation error upon failure.
/// </summary>
public interface IRule
{
    /// <summary>
    /// Constructs and returns the validation error associated with this rule's failure.
    /// </summary>
    /// <returns>A <see cref="ValidationError"/> instance representing the failure details.</returns>
    ValidationError GetError();
    
    /// <summary>
    /// Evaluates the validation rule logic.
    /// </summary>
    /// <returns><see langword="true"/> if the rule passes; otherwise, <see langword="false"/>.</returns>
    bool Validate();
    
    /// <summary>
    /// Sets the target object name on the rule for contextualized error messages.
    /// </summary>
    string ObjectName { set; }
}