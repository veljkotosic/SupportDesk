namespace SupportDesk.WebApi.Controllers.v1.Note.Requests;

public sealed record AddNoteRequest(Guid TicketId, string Text);