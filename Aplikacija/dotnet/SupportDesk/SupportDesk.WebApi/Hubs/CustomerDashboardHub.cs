using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Infrastructure.Auth;

namespace SupportDesk.WebApi.Hubs;

[Authorize(Policy = Policies.CustomerOnly)]
public class CustomerDashboardHub : Hub
{
    private readonly IUserContext _userContext;

    public CustomerDashboardHub(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _userContext.GetCurrentUserId();
        
        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _userContext.GetCurrentUserId();
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        
        await base.OnDisconnectedAsync(exception);
    }
}