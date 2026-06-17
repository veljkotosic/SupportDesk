using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetOrganizationTickets;

public class GetOrganizationTicketsRequestHandler
    : IRequestHandler<GetOrganizationTicketsRequest, GetOrganizationTicketsResult>
{
    private readonly SupportDeskDbContext _context;
    private readonly IUserContext _userContext;

    public GetOrganizationTicketsRequestHandler(
        SupportDeskDbContext context,
        IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<GetOrganizationTicketsResult> HandleAsync(
        GetOrganizationTicketsRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;

        var organizationTickets = _context.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.OrganizationId == organizationId);

        var allCount = await organizationTickets.CountAsync(cancellationToken);
        var openCount = await organizationTickets.CountAsync(ticket => ticket.Status == TicketStatus.Open, cancellationToken);
        var assignedCount = await organizationTickets.CountAsync(ticket => ticket.Status == TicketStatus.Assigned, cancellationToken);
        var closedCount = await organizationTickets.CountAsync(ticket => ticket.Status == TicketStatus.Closed, cancellationToken);

        var filteredTickets = organizationTickets;
        var search = request.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            var parsedTicketId = Guid.TryParse(search, out var ticketId) ? ticketId : (Guid?)null;

            filteredTickets = filteredTickets.Where(ticket =>
                (parsedTicketId.HasValue && ticket.Id == parsedTicketId.Value) ||
                EF.Functions.ILike(ticket.Subject, searchPattern) ||
                EF.Functions.ILike(ticket.Customer.UserName!, searchPattern) ||
                EF.Functions.ILike(ticket.Category.Name, searchPattern));
        }

        if (request.Status.HasValue)
        {
            filteredTickets = filteredTickets.Where(ticket => ticket.Status == request.Status.Value);
        }

        if (request.Priority.HasValue)
        {
            filteredTickets = filteredTickets.Where(ticket => ticket.Priority == request.Priority.Value);
        }

        var totalCount = await filteredTickets.CountAsync(cancellationToken);

        var orderedTickets = request.SortBy switch
        {
            "oldest" => filteredTickets.OrderBy(ticket => ticket.LastMessageAt).ThenBy(ticket => ticket.Id),
            "priority" => filteredTickets.OrderByDescending(ticket => ticket.Priority)
                .ThenByDescending(ticket => ticket.LastMessageAt)
                .ThenByDescending(ticket => ticket.Id),
            _ => filteredTickets.OrderByDescending(ticket => ticket.LastMessageAt).ThenByDescending(ticket => ticket.Id)
        };

        var tickets = await orderedTickets
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(ticket => new OrganizationTicketListItemDto(
                ticket.Id,
                ticket.CategoryId,
                ticket.Category.Name,
                ticket.CustomerId,
                ticket.Customer.UserName!,
                ticket.Customer.Email!,
                ticket.SupportAgentId,
                ticket.SupportAgent != null ? ticket.SupportAgent.UserName : null,
                ticket.Status,
                ticket.Priority,
                ticket.Feedback,
                ticket.Subject,
                ticket.LastMessageAt))
            .ToListAsync(cancellationToken);

        return new GetOrganizationTicketsResult(
            tickets,
            totalCount,
            allCount,
            openCount,
            assignedCount,
            closedCount);
    }
}
