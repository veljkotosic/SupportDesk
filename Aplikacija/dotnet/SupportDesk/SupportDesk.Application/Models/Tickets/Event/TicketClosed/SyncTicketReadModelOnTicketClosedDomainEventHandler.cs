using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketClosed;

internal sealed class SyncTicketReadModelOnTicketClosedDomainEventHandler
    : IDomainEventHandler<TicketClosedDomainEvent>
{
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketClosedDomainEventHandler(ITicketReadModelService ticketReadModelService)
    {
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketClosedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _ticketReadModelService.UpdateTicketStatusAsync(
            domainEvent.TicketId.IdValue,
            TicketStatus.Closed,
            domainEvent.ClosedAt.ClosedAtValue,
            cancellationToken);
    }
}
