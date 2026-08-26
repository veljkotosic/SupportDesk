namespace SupportDesk.Domain.Abstract.Validation.Rule;

/// <summary>
/// Provides an abstract base implementation of <see cref="IRule"/> for building reusable validation rules.
/// </summary>
public abstract class AbstractRule : IRule
{
    /// <summary>
    /// Gets the unique error code associated with this validation failure.
    /// </summary>
    protected abstract string ErrorCode { get; }
    
    /// <summary>
    /// Gets the human-readable error message describing the validation failure.
    /// </summary>
    protected abstract string ErrorMessage { get; }

    /// <summary>
    /// Gets or sets the name of the object or property being validated for contextual error reporting. Defaults to "Unknown".
    /// </summary>
    public string ObjectName { protected get; set; } = "Unknown";
    
    /// <summary>
    /// Creates and returns a <see cref="ValidationError"/> instance representing this rule's failure.
    /// </summary>
    /// <returns>A new <see cref="ValidationError"/> containing the error code and error message.</returns>
    public ValidationError GetError()
    {
        return new ValidationError(ErrorCode, ErrorMessage);
    }

    /// <summary>
    /// Evaluates the validation rule logic against the target value.
    /// </summary>
    /// <returns><see langword="true"/> if the validation condition is met; otherwise, <see langword="false"/>.</returns>
    public abstract bool Validate();
}