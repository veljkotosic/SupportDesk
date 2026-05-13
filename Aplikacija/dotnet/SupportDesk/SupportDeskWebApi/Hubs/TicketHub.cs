using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Ticket.CreateTicket;

namespace SupportDeskWebApi.Hubs;

[Authorize]
public class TicketHub : Hub
{
    private readonly IDispatcher _dispatcher;

    public TicketHub(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    public async Task JoinTicket(string ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted); 
    }
    
    public async Task LeaveTicket(string ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);
    }
}