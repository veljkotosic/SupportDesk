using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Tickets.Command.ReadAllNotifications;

public sealed record ReadAllNotificationsCommand(Guid TicketId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.ReadNotifications
        ];
    }
}