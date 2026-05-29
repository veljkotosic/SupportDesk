namespace SupportDeskWebApi.Requests.Note.Common;

public record NoteDetailsDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUsername,
    string Text,
    DateTime CreatedAt);