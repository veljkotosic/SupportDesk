namespace SupportDeskWebApi.Requests.Note.AddNote;

public record AddNoteDto(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    Guid AuthorId,
    string Text, 
    DateTime CreatedAt);