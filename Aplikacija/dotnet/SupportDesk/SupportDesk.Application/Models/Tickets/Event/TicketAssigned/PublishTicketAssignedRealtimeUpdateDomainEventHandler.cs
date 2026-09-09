using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Event.TicketAssigned;

internal sealed class PublishTicketAssignedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<TicketAssignedDomainEvent>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishTicketAssignedRealtimeUpdateDomainEventHandler(
        IApplicationDbContext applicationDbContext,
        IRealtimePublisher realtimePublisher)
    {
        _applicationDbContext = applicationDbContext;
        _realtimePublisher = realtimePublisher;
    }
    
    public async Task HandleAsync(TicketAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var ticketAssignedInformation = await GetQuery(domainEvent.TicketId).FirstOrDefaultAsync(cancellationToken);
        
        if (ticketAssignedInformation is null)
        {
            throw new ValidationException(TicketErrors.NotFound(domainEvent.TicketId));
        }
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.OrganizationDashboard,
            domainEvent.OrganizationId.IdValue.ToString(),
            "TicketAssigned",
            ticketAssignedInformation,
            cancellationToken);       
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.CustomerDashboard,
            domainEvent.CustomerId.IdValue.ToString(),
            "TicketAssigned",
            ticketAssignedInformation,
            cancellationToken);       
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.Ticket,
            domainEvent.TicketId.IdValue.ToString(),
            "TicketAssigned",
            ticketAssignedInformation,
            cancellationToken);       
    }

    private IQueryable<GetTicketAssignedInformationQueryResult> GetQuery(TicketId ticketId)
    {
        var queryable =
            from ticket in _applicationDbContext.Tickets.AsNoTracking()
            where ticket.Id == ticketId && ticket.AssignedAt != null

            join supportAgent in _applicationDbContext.DomainUsers.AsNoTracking()
                on ticket.SupportAgentId equals supportAgent.Id

            select new GetTicketAssignedInformationQueryResult(
                ticket.Id.IdValue,
                supportAgent.Id.IdValue,
                supportAgent.UserName.UserNameValue,
                ticket.AssignedAt!.AssignedAtValue);
        
        return queryable;
    }
}

internal sealed record GetTicketAssignedInformationQueryResult(
    Guid TicketId,
    Guid SupportAgentId,
    string SupportAgentUsername,
    DateTime AssignedAt);