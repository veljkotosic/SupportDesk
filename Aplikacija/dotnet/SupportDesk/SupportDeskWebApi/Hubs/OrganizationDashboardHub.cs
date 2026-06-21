using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;

namespace SupportDeskWebApi.Hubs;

[Authorize(Roles = "OrganizationAdmin, SupportAgent")]
public class OrganizationDashboardHub : Hub
{
    private readonly IUserContext _userContext;

    public OrganizationDashboardHub(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task StartLiveUpdates()
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            _userContext.GetCurrentUsersOrganizationId().ToString()!,
            Context.ConnectionAborted);
    }
    
    public async Task StopLiveUpdates()
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, 
            _userContext.GetCurrentUsersOrganizationId().ToString()!,
            Context.ConnectionAborted);
    }
}