using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification.Events;

public sealed record TicketNotificationCreatedDomainEvent(
    TicketNotificationId TicketNotificationId,
    UserId CustomerId
    ) : IDomainEvent;