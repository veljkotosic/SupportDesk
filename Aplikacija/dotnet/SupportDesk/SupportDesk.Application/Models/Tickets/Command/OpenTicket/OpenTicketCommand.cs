using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Command.OpenTicket;

public sealed record OpenTicketCommand(
    Guid OrganizationId,
    Guid CategoryId,
    TicketPriority Priority,
    string Subject,
    string InitialMessage
    ) : ICommand<OpenTicketCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.Open,
            Permissions.Messages.Send
        ];
    }
}