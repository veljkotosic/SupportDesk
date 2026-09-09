using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Abstract.ValueObject;

/// <summary>
/// Provides a base record for immutable domain value objects with built-in validation support.
/// </summary>
public abstract record AbstractValueObject : IValidatable
{
    /// <summary>
    /// Gets the collection of validation rules applicable to this value object.
    /// </summary>
    /// <returns>A collection of <see cref="IRule"/> instances. Defaults to an empty collection.</returns>
    public virtual ICollection<IRule> GetValidationRules()
    {
        return [];
    }
    
    /// <summary>
    /// Validates the current state of the value object against its configured validation rules.
    /// </summary>
    /// <exception cref="SupportDesk.Domain.Abstract.Validation.ValidationException">Thrown when one or more validation rules fail.</exception>
    public void ValidateValueObject() => (this as IValidatable).Validate();
}