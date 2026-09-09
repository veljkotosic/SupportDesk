using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Abstract.Validation;

/// <summary>
/// Defines a contract for domain models or value objects that can validate their state using a collection of validation rules.
/// </summary>
public interface  IValidatable
{
    /// <summary>
    /// Gets the collection of validation rules to evaluate against this instance.
    /// </summary>
    /// <returns>A collection of <see cref="IRule"/> instances.</returns>
    ICollection<IRule> GetValidationRules();

    /// <summary>
    /// Validates the object state against all defined rules and throws a validation exception if any rule fails.
    /// </summary>
    /// <exception cref="SupportDesk.Domain.Abstract.Validation.ValidationException">Thrown when one or more validation rules fail.</exception>
    void Validate()
    {
        var rules = GetValidationRules();
        var validator = new Validator(rules, ValidatableObjectName);
        
        validator.Validate();

        if (!validator.Success)
        {
            throw validator.GetValidationException();
        }
    }
    
    /// <summary>
    /// Gets the display name of the object being validated, used in validation error messages. Defaults to the type name.
    /// </summary>
    string ValidatableObjectName => GetType().Name;
}

/// <summary>
/// Defines a contract for domain objects that require an external context instance to evaluate their validation rules.
/// </summary>
/// <typeparam name="TContext">The type of context required during validation.</typeparam>
public interface IValidatable<in TContext>
{
    /// <summary>
    /// Gets the collection of validation rules to evaluate against this instance using the provided context.
    /// </summary>
    /// <param name="context">The contextual data or dependency used for rule evaluation.</param>
    /// <returns>A collection of <see cref="IRule"/> instances.</returns>
    ICollection<IRule> GetValidationRules(TContext context);

    /// <summary>
    /// Validates the object state using the provided context and throws a validation exception if any rule fails.
    /// </summary>
    /// <param name="context">The contextual data or dependency used for rule evaluation.</param>
    /// <exception cref="SupportDesk.Domain.Abstract.Validation.ValidationException">Thrown when one or more validation rules fail.</exception>
    void Validate(TContext context)
    {
        var rules = GetValidationRules(context);
        var validator = new Validator(rules, ValidatableObjectName);
        
        validator.Validate();

        if (!validator.Success)
        {
            throw validator.GetValidationException();
        }
    }
    
    /// <summary>
    /// Gets the display name of the object being validated, used in validation error messages. Defaults to the type name.
    /// </summary>
    string ValidatableObjectName => GetType().Name;
}