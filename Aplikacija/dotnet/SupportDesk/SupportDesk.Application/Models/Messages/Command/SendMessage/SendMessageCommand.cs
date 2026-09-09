using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Messages.Command.SendMessage;

public sealed record SendMessageCommand(Guid TicketId, string Text) : ICommand<SendMessageCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Messages.Send
        ];
    }
}