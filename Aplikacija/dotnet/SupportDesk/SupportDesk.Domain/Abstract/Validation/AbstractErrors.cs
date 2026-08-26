using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Abstract.Validation;

/// <summary>
/// Provides base functionality and standard factory methods for generating domain validation errors for specific domain models.
/// </summary>
/// <typeparam name="TModel">The domain model type associated with these errors. Must derive from <see cref="AbstractDomainModel{TId}"/>.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type used by the domain model. Must derive from <see cref="DomainId"/>.</typeparam>
public abstract class AbstractErrors<TModel, TId>
    where TModel : AbstractDomainModel<TId> 
    where TId : DomainId
{
    /// <summary>
    /// Gets the class name of the associated domain model type.
    /// </summary>
    /// <returns>The string name of <typeparamref name="TModel"/>.</returns>
    public static string GetModelName()
    {
        return typeof(TModel).Name;
    }
    
    /// <summary>
    /// Gets the standardized lower-case prefix used for error codes for this domain model.
    /// </summary>
    /// <returns>A lower-case prefix string representing the model name.</returns>
    public static string Prefix()
    {
        return GetModelName().ToLower();
    }

    /// <summary>
    /// Constructs a fully qualified error code prefixed with the domain model prefix.
    /// </summary>
    /// <param name="code">The specific error code suffix.</param>
    /// <returns>A formatted error code string in the format <c>{prefix}_{code}</c>.</returns>
    protected static string GetFullCode(string code)
    {
        return $"{Prefix()}_{code}";
    }

    /// <summary>
    /// Creates a new <see cref="ValidationError"/> with a prefixed error code and the specified error message.
    /// </summary>
    /// <param name="code">The error code suffix.</param>
    /// <param name="errorMessage">The descriptive error message.</param>
    /// <returns>A new <see cref="ValidationError"/> instance.</returns>
    protected static ValidationError CreateValidationError(string code, string errorMessage)
    {
        return new ValidationError(GetFullCode(code), errorMessage);
    }

    /// <summary>
    /// Creates a standardized "not found" validation error when an entity cannot be found by its identifier.
    /// </summary>
    /// <param name="id">The identifier that was not found.</param>
    /// <returns>A <see cref="ValidationError"/> indicating the model with the given id was not found.</returns>
    public static ValidationError NotFound(TId id)
    {
        const string code = "not_found";
        string message = $"{GetModelName()} with id '{id}' was not found!";
        
        return CreateValidationError(code,message);
    }

    /// <summary>
    /// Creates a standardized "not found" validation error when an entity cannot be found by a specific property value.
    /// </summary>
    /// <param name="propertyName">The name of the property queried.</param>
    /// <param name="propertyValue">The value of the property queried.</param>
    /// <returns>A <see cref="ValidationError"/> indicating the model with the given property value was not found.</returns>
    public static ValidationError NotFound(string propertyName, object propertyValue)
    {
        const string code = "not_found";
        string message = $"{GetModelName()} with {propertyName} '{propertyValue}' was not found!";
        
        return CreateValidationError(code,message);   
    }
}