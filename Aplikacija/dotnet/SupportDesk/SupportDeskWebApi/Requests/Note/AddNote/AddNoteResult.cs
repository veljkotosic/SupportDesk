using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Note.AddNote;

public record AddNoteResult(Guid NoteId) : IRequestResult;