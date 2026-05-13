using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;

namespace SupportDeskWebApi.Hubs;

[Authorize(Roles = "Customer")]
public class CustomerDashboardHub : Hub
{
    private readonly IUserContext _userContext;

    public CustomerDashboardHub(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task StartLiveUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, _userContext.GetCurrentUserId().ToString());
    }
    
    public async Task StopLiveUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, _userContext.GetCurrentUserId().ToString());
    }
}