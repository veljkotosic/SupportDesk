using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Infrastructure.Auth;

namespace SupportDesk.WebApi.Hubs;

[Authorize(Policy = Policies.OrganizationMemberOnly)]
public class OrganizationDashboardHub : Hub
{
    private readonly ITenantContext _tenantContext;

    public OrganizationDashboardHub(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override async Task OnConnectedAsync()
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, organizationId.ToString());
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, organizationId.ToString());

        await base.OnDisconnectedAsync(exception);
    }
}