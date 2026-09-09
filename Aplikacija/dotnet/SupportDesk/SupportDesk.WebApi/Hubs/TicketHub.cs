using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SupportDesk.WebApi.Hubs;

[Authorize]
public class TicketHub : Hub
{
    public async Task JoinTicket(string ticketId)
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (!string.IsNullOrEmpty(organizationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);       
        }
    }
    
    public async Task LeaveTicket(string ticketId)
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (!string.IsNullOrEmpty(organizationId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);       
        }
    }
}