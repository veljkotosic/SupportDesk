using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSupportAgents;

public class GetSupportAgentsRequestHandler
    : IRequestHandler<GetSupportAgentsRequest, GetSupportAgentsResult>
{
    private readonly SupportDeskDbContext _context;
    private readonly IUserContext _userContext;

    public GetSupportAgentsRequestHandler(
        SupportDeskDbContext context,
        IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<GetSupportAgentsResult> HandleAsync(
        GetSupportAgentsRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;

        var agents = await _context.Users
            .AsNoTracking()
            .Where(user => user.OrganizationId == organizationId && user.Type == UserType.SupportAgent)
            .OrderBy(user => user.UserName)
            .Select(user => new SupportAgentDto(
                user.Id,
                user.UserName!,
                user.Email!,
                user.AssignedTickets.Count(ticket => ticket.Status == TicketStatus.Assigned),
                user.AssignedTickets.Count(ticket => ticket.Status == TicketStatus.Closed),
                user.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetSupportAgentsResult(agents);
    }
}
