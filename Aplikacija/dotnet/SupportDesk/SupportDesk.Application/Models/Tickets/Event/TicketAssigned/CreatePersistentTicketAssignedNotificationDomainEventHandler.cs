using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.TicketNotification.Repository;

namespace SupportDesk.Application.Models.Tickets.Event.TicketAssigned;

internal sealed class CreatePersistentTicketAssignedNotificationDomainEventHandler
    : IDomainEventHandler<TicketAssignedDomainEvent>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITicketNotificationRepository  _ticketNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePersistentTicketAssignedNotificationDomainEventHandler(
        TimeProvider timeProvider,
        ITicketNotificationRepository ticketNotificationRepository,
        IUnitOfWork unitOfWork)
    {
        _timeProvider = timeProvider;
        _ticketNotificationRepository = ticketNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TicketAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string notificationText = "Support agent is reviewing your ticket.";

        var ticketNotification = TicketNotification.Create(
            domainEvent.OrganizationId.IdValue,
            domainEvent.TicketId.IdValue,
            notificationText,
            domainEvent.CustomerId.IdValue,
            _timeProvider);
        
        await _ticketNotificationRepository.SaveAsync(ticketNotification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }
}