using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Infrastructure.Auth;

namespace SupportDesk.WebApi.Hubs;

[Authorize(Policy = Policies.OrganizationMemberOnly)]
public class OrganizationDashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;
            
        if (!string.IsNullOrEmpty(organizationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, organizationId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;

        if (!string.IsNullOrEmpty(organizationId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, organizationId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}