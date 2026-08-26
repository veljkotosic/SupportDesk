using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.TicketNotification.Events;

public sealed record TicketNotificationCreatedDomainEvent(Guid TicketNotificationId) : IDomainEvent;