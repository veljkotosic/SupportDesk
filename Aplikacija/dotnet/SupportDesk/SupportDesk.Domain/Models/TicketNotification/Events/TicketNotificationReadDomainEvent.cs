using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification.Events;

public sealed record TicketNotificationReadDomainEvent(TicketNotificationId TicketNotificationId) : IDomainEvent;