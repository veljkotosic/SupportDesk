using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Events;

public sealed record TicketOpenedDomainEvent(TicketId TicketId) : IDomainEvent;