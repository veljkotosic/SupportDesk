using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly SupportDeskDbContext _dbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IDomainEventCollector _domainEventCollector;

    public EfUnitOfWork(
        SupportDeskDbContext dbContext,
        IDomainEventDispatcher domainEventDispatcher,
        IDomainEventCollector domainEventCollector)
    {
        _dbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
        _domainEventCollector = domainEventCollector;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = _domainEventCollector.GetDomainEvents();
        _domainEventCollector.ClearDomainEvents();
        
        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }
}