using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Abstract;

/// <summary>
/// Provides a base class for domain entities and aggregate roots, encapsulating identity, domain event management, and self-validation.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier type used by the domain model. Must derive from <see cref="DomainId"/>.</typeparam>
public abstract class AbstractDomainModel<TId> : IValidatable
    where TId : DomainId
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    /// <summary>
    /// Gets the strongly-typed unique identifier of the domain entity.
    /// </summary>
    public TId Id { get; init; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractDomainModel{TId}"/> class for ORM frameworks.
    /// </summary>
    protected AbstractDomainModel()
    {
        
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractDomainModel{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique strongly typed identifier of the entity.</param>
    protected AbstractDomainModel(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the type name of the domain model.
    /// </summary>
    /// <returns>A string representing the domain model class name.</returns>
    public string GetModelName()
    {
        return GetType().Name;
    }

    /// <summary>
    /// Retrieves a read-only snapshot of all domain events raised by this entity instance.
    /// </summary>
    /// <returns>A read-only list containing accumulated <see cref="IDomainEvent"/> instances.</returns>
    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
    
    /// <summary>
    /// Clears all accumulated domain events from this entity.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    /// <summary>
    /// Records a domain event to be dispatched when the entity changes are committed.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to register.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    
    /// <summary>
    /// Gets the collection of validation rules applicable to this domain entity.
    /// </summary>
    /// <returns>A collection of <see cref="IRule"/> instances. Defaults to an empty collection.</returns>
    public virtual ICollection<IRule> GetValidationRules()
    {
        return [];
    }
    
    /// <summary>
    /// Validates the current state of the domain entity against its configured validation rules.
    /// </summary>
    /// <exception cref="SupportDesk.Domain.Abstract.Validation.ValidationException">Thrown when one or more validation rules fail.</exception>
    public void ValidateModel() => (this as IValidatable).Validate();
}