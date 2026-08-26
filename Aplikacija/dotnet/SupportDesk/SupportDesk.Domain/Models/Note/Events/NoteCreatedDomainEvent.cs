using SupportDesk.Domain.Abstract;

namespace SupportDesk.Domain.Models.Note.Events;

public sealed record NoteCreatedDomainEvent(Guid NoteId) : IDomainEvent;