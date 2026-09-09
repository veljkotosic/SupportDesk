using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification.ValueObjects;

public sealed record TicketNotificationId(Guid IdValue) : DomainId(IdValue)
{
    public static TicketNotificationId NewId() => new(Guid.NewGuid());
}