using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Note.ValueObjects;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Note.Events;

public sealed record NoteCreatedDomainEvent(NoteId NoteId, TicketId TicketId) : IDomainEvent;