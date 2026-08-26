using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Abstract.Repository;

/// <summary>
/// Defines generic repository operations for managing the lifecycle and persistence of domain aggregate roots/entities.
/// </summary>
/// <typeparam name="TModel">The domain model type managed by this repository.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type used by the domain model.</typeparam>
public interface IAbstractRepository<TModel, in TId>
    where TModel : AbstractDomainModel<TId>
    where TId : DomainId
{
    /// <summary>
    /// Asynchronously retrieves a domain model instance by its unique strongly-typed identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the matched domain model instance, or <see langword="null"/> if no entity was found.
    /// </returns>
    Task<TModel?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously persists or queues the specified domain model instance for persistence.
    /// </summary>
    /// <param name="model">The domain model instance to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SaveAsync(TModel model, CancellationToken cancellationToken = default);
}