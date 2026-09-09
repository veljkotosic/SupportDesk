using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Application.Models.Tickets.Enums;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

public sealed record GetOrganizationTicketsQuery(
    int Skip = 0,
    int Take = 10,
    string? SearchTerm = null,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    TicketSortOption SortBy = TicketSortOption.Latest
    ) : IQuery<GetOrganizationTicketsQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.GetOrganizationTickets
        ];
    }
}