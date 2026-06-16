using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;

namespace SupportDeskWebApi.Hubs;

[Authorize]
public class TicketHub : Hub
{
    private readonly IUserContext _userContext;
    private readonly SupportDeskDbContext _context;

    public TicketHub(IUserContext userContext, SupportDeskDbContext context)
    {
        _userContext = userContext;
        _context = context;
    }
    
    public async Task JoinTicket(string ticketId)
    {
        if (!Guid.TryParse(ticketId, out var parsedTicketId) ||
            !await _context.Tickets.AnyAsync(ticket => ticket.Id == parsedTicketId, Context.ConnectionAborted))
        {
            throw new HubException("Ticket not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (_userContext.GetCurrentUsersOrganizationId() is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);
        }
    }
    
    public async Task LeaveTicket(string ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (_userContext.GetCurrentUsersOrganizationId() is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);
        }
    }
}
