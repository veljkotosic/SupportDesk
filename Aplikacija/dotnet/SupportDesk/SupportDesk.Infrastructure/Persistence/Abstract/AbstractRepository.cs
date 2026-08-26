using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Abstract;

public abstract class AbstractRepository<TDomainModel, TId> : IAbstractRepository<TDomainModel, TId>
    where TDomainModel : AbstractDomainModel<TId>
    where TId : DomainId
{
    protected readonly SupportDeskDbContext Context;
    protected readonly DbSet<TDomainModel> DbSet;
    private readonly IDomainEventCollector _domainEventCollector;
    
    protected AbstractRepository(SupportDeskDbContext context, IDomainEventCollector domainEventCollector)
    {
        Context = context;
        DbSet = Context.Set<TDomainModel>();
        _domainEventCollector = domainEventCollector;
    }
    
    public async Task<TDomainModel?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public async Task SaveAsync(TDomainModel model, CancellationToken cancellationToken = default)
    {
        var existingModel = await DbSet.FindAsync([model.Id], cancellationToken);

        if (existingModel is null)
        {
            await DbSet.AddAsync(model, cancellationToken);
        }
        else
        {
            Context.Entry(existingModel).CurrentValues.SetValues(model);
        }
        
        CollectDomainEvents(model);
    }
    
    private void CollectDomainEvents(TDomainModel model)
    {
        var domainEvents = model.GetDomainEvents();

        if (domainEvents.Count == 0)
        {
            return;
        }

        _domainEventCollector.AddDomainEvents(domainEvents);
        model.ClearDomainEvents();
    }
}