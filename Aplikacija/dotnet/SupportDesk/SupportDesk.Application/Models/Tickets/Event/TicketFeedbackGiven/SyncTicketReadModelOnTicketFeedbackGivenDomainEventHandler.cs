using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketFeedbackGiven;

internal sealed class SyncTicketReadModelOnTicketFeedbackGivenDomainEventHandler
    : IDomainEventHandler<TicketFeedbackGivenDomainEvent>
{
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketFeedbackGivenDomainEventHandler(ITicketReadModelService ticketReadModelService)
    {
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketFeedbackGivenDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _ticketReadModelService.UpdateTicketFeedbackAsync(
            domainEvent.TicketId.IdValue,
            domainEvent.Feedback,
            cancellationToken);
    }
}
