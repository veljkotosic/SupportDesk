using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetOrganizationTickets;

public record GetOrganizationTicketsRequest(
    int Skip,
    int Take,
    string? Search,
    TicketStatus? Status,
    TicketPriority? Priority,
    string SortBy) : IRequest<GetOrganizationTicketsResult>;
