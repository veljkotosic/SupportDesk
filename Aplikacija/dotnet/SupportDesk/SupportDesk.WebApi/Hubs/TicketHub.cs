using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportDesk.Application.Abstract.Auth.TenantContext;

namespace SupportDesk.WebApi.Hubs;

[Authorize]
public class TicketHub : Hub
{
    private readonly ITenantContext _tenantContext;

    public TicketHub(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public async Task JoinTicket(string ticketId)
    {
        var organizationId = _tenantContext.GetCurrentOrganizationId();
        
        await Groups.AddToGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (organizationId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);       
        }
    }
    
    public async Task LeaveTicket(string ticketId)
    {
        var organizationId = _tenantContext.GetCurrentOrganizationId();
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId, Context.ConnectionAborted);

        if (organizationId is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{ticketId}:organization", Context.ConnectionAborted);       
        }
    }
}