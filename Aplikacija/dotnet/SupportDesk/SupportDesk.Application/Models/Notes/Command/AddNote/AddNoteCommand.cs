using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Notes.Command.AddNote;

public sealed record AddNoteCommand(Guid TicketId, string Text) : ICommand<AddNoteCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Notes.Add
        ];
    }
}