using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Abstract.Validation;

/// <summary>
/// Defines a contract for domain objects requiring sequential, multi-stage validation where subsequent stages are only evaluated if prior stages pass.
/// </summary>
/// <typeparam name="TContext">The type of context required during validation.</typeparam>
public interface IStageValidatable<in TContext>
{
    /// <summary>
    /// Gets the sequence of validation stages, where each stage is represented by a collection of <see cref="IRule"/> instances.
    /// </summary>
    /// <param name="context">The contextual data or dependency used for rule evaluation.</param>
    /// <returns>A read-only list of rule collections grouped by validation stage.</returns>
    IReadOnlyList<ICollection<IRule>> GetValidationStages(TContext context);

    /// <summary>
    /// Sequentially executes each validation stage against the provided context, stopping and throwing on the first failing stage.
    /// </summary>
    /// <param name="context">The contextual data or dependency used for rule evaluation.</param>
    /// <exception cref="SupportDesk.Domain.Abstract.Validation.ValidationException">Thrown when one or more validation rules fail in any stage.</exception>
    void Validate(TContext context)
    {
        var stages = GetValidationStages(context);

        foreach (var stageRules in stages)
        {
            if (stageRules.Count == 0)
            {
                continue;
            }

            var validator = new Validator(stageRules, ValidatableObjectName);
            validator.Validate();

            if (!validator.Success)
            {
                throw validator.GetValidationException();
            }
        }
    }
    
    /// <summary>
    /// Gets the display name of the object being validated, used in validation error messages. Defaults to the type name.
    /// </summary>
    string ValidatableObjectName => GetType().Name;
}