using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Command.GiveFeedback;

public sealed record GiveFeedbackCommand(Guid TicketId, TicketFeedback Feedback) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.GiveFeedback
        ];
    }
}