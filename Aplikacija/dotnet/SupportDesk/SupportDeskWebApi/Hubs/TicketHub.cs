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
    
    public async Task JoinTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId.ToString(), cancellationToken);
    }
    
    public async Task LeaveTicket(Guid ticketId, CancellationToken cancellationToken)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId.ToString(), cancellationToken);
    }
}