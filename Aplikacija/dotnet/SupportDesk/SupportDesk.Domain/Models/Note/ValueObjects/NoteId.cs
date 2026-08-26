using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Models.Note.ValueObjects;

public sealed record NoteId(Guid IdValue) : DomainId(IdValue)
{
    public static NoteId NewId() => new(Guid.NewGuid());
}