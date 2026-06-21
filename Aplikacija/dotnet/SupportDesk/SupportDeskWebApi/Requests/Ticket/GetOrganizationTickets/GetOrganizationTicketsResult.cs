using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetOrganizationTickets;

public record GetOrganizationTicketsResult(
    List<OrganizationTicketListItemDto> Tickets,
    int TotalCount,
    int AllCount,
    int OpenCount,
    int AssignedCount,
    int ClosedCount) : IRequestResult;
