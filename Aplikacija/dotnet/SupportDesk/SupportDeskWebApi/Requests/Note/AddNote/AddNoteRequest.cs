using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Note.AddNote;

public record AddNoteRequest(Guid TicketId, string Text) : IRequest<AddNoteResult>;