using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Models.Notes.Command.AddNote;

public sealed record AddNoteCommandResult(Guid NoteId) : ICommandResult;