using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Infrastructure.Event;

public class DomainEventCollector : IDomainEventCollector
{
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public void AddDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        _domainEvents.AddRange(domainEvents);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}