using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

public sealed record GetOrganizationTicketsQueryResult(
    IReadOnlyList<OrganizationTicketListingDto> Tickets,
    int TotalCount,
    int AllCount,
    int OpenCount,
    int AssignedCount,
    int ClosedCount
) : IQueryResult;