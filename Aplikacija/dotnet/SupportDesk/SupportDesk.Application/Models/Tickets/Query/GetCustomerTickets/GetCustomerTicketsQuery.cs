using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

public sealed record GetCustomerTicketsQuery(
    int Skip = 0,
    int Take = 10,
    string? SearchTerm = null,
    TicketStatus? Status = null
    ) : IQuery<GetCustomerTicketsQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Tickets.GetCustomerTickets
        ];
    }
}