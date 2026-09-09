using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketId(Guid IdValue) : DomainId(IdValue)
{
    public static TicketId NewId() => new(Guid.NewGuid());
}