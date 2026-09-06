using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Note.ValueObjects;

namespace SupportDesk.Domain.Models.Note.Events;

public sealed record NoteCreatedDomainEvent(NoteId NoteId) : IDomainEvent;