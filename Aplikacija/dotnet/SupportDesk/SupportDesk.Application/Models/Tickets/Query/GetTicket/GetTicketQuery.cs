using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicket;

public sealed record GetTicketQuery(Guid TicketId) : IQuery<GetTicketQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.Get
        ];
    }
}