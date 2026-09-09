using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketClosed;

internal sealed class PublishTicketClosedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<TicketClosedDomainEvent>
{
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishTicketClosedRealtimeUpdateDomainEventHandler(IRealtimePublisher realtimePublisher)
    {
        _realtimePublisher = realtimePublisher;
    }

    public async Task HandleAsync(TicketClosedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var ticketClosedInformation = new TicketClosedInfoDto(
            domainEvent.TicketId.IdValue,
            domainEvent.SupportAgentId.IdValue,
            domainEvent.ClosedAt.ClosedAtValue);
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.OrganizationDashboard,
            domainEvent.OrganizationId.IdValue.ToString(),
            "TicketClosed",
            ticketClosedInformation,
            cancellationToken);
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.CustomerDashboard,
            domainEvent.CustomerId.IdValue.ToString(),
            "TicketClosed",
            ticketClosedInformation,
            cancellationToken);
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.Ticket,
            domainEvent.TicketId.IdValue.ToString(),
            "TicketClosed",
            ticketClosedInformation,
            cancellationToken);
    }
}

internal sealed record TicketClosedInfoDto(
    Guid TicketId,
    Guid SupportAgentId,
    DateTime ClosedAt);