using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Tickets.Command.AssignTicket;

public sealed record AssignTicketCommand(Guid TicketId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.Assign
        ];
    }
}