using SupportDesk.Domain.Abstract;

namespace SupportDesk.Application.Abstract.Event;

/// <summary>
/// Defines a collector used to aggregate domain events raised during business operations.
/// </summary>
/// <remarks>
/// Serves as an in-memory buffer to accumulate domain events across operations (e.g., from entities or repositories) 
/// before they are dispatched to their respective handlers.
/// </remarks>
public interface IDomainEventCollector
{
    /// <summary>
    /// Adds a sequence of domain events to the collector.
    /// </summary>
    /// <param name="domainEvents">The collection of domain events to append.</param>
    void AddDomainEvents(IEnumerable<IDomainEvent> domainEvents);
    
    /// <summary>
    /// Retrieves all accumulated domain events.
    /// </summary>
    /// <returns>A read-only list of collected <see cref="IDomainEvent"/> instances.</returns>
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    
    /// <summary>
    /// Clears all accumulated domain events from the collector.
    /// </summary>
    void ClearDomainEvents();   
}