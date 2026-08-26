using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.Ticket.Events;

public sealed record TicketCreatedDomainEvent(Guid TicketId) : IDomainEvent;