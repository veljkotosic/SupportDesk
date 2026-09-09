namespace SupportDesk.Domain.Abstract.Validation.Rule;

/// <summary>
/// Defines static metadata for a validation rule, including unique error codes.
/// </summary>
public interface IRuleMetadata
{
    /// <summary>
    /// Gets the standardized error code string representing the validation rule failure.
    /// </summary>
    static abstract string ErrorCodeString { get; }
}