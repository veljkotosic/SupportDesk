using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo;

public sealed record GetTicketViewInfoQuery(Guid TicketId) : IQuery<GetTicketViewInfoQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.View
        ];
    }
}