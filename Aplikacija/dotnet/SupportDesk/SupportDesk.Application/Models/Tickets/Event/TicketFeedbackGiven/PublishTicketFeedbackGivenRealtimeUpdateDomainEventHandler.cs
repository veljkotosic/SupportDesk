using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketFeedbackGiven;

internal sealed class PublishTicketFeedbackGivenRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<TicketFeedbackGivenDomainEvent>
{
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishTicketFeedbackGivenRealtimeUpdateDomainEventHandler(IRealtimePublisher realtimePublisher)
    {
        _realtimePublisher = realtimePublisher;
    }

    public async Task HandleAsync(TicketFeedbackGivenDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var feedback = new TicketFeedbackInfoDto(domainEvent.TicketId.IdValue, domainEvent.Feedback);
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.OrganizationDashboard,
            domainEvent.OrganizationId.IdValue.ToString(),
            "TicketFeedback",
            feedback,
            cancellationToken);     
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.Ticket,
            domainEvent.TicketId.IdValue.ToString(),
            "TicketFeedback",
            feedback,
            cancellationToken);       
    }
}

internal sealed record TicketFeedbackInfoDto(Guid TicketId, TicketFeedback Feedback);