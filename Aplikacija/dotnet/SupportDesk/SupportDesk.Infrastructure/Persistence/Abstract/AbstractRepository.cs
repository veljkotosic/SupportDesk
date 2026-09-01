using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Infrastructure.Messaging.Outbox;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Abstract;

public abstract class AbstractRepository<TDomainModel, TId> : IAbstractRepository<TDomainModel, TId>
    where TDomainModel : AbstractDomainModel<TId>
    where TId : DomainId
{
    protected readonly SupportDeskDbContext Context;
    protected readonly DbSet<TDomainModel> DbSet;
    
    private readonly IServiceProvider _serviceProvider;
    
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    
    protected AbstractRepository(
        SupportDeskDbContext context,
        IServiceProvider serviceProvider,
        IUserContext userContext,
        ITenantContext tenantContext)
    {
        Context = context;
        DbSet = Context.Set<TDomainModel>();
        
        _serviceProvider = serviceProvider;
        
        _userContext = userContext;
        _tenantContext = tenantContext;
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
        
        var domainEvents = model.GetDomainEvents();

        if (domainEvents.Count > 0)
        {
            foreach (var domainEvent in domainEvents)
            {
                var eventType = domainEvent.GetType();
                var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

                var handlers = _serviceProvider.GetServices(handlerInterfaceType);

                foreach (var handler in handlers)
                {
                    if (handler is null)
                    {
                        continue;
                    }

                    var handlerType = handler.GetType();

                    var outboxMessage = new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        HandlerType = handlerType.AssemblyQualifiedName ?? handlerType.FullName!,
                        EventType = eventType.AssemblyQualifiedName ?? eventType.FullName!,
                        Payload = JsonSerializer.Serialize(domainEvent, eventType),
                        UserId = _userContext.TryGetCurrentUserId(),
                        OrganizationId = _tenantContext.GetCurrentOrganizationId(),
                        OccurredOnUtc = DateTime.UtcNow,
                        ProcessedOnUtc = null,
                        Error = null
                    };
                    
                    await Context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
                }
            }
            
            model.ClearDomainEvents();
        }
    }
}