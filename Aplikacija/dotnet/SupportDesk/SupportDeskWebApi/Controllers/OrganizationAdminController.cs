using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;
using SupportDeskWebApi.Requests.OrganizationAdmin.GetSupportAgents;
using SupportDeskWebApi.Requests.User.OrganizationAdmin.GenerateSupportAgentInviteCode;
using SupportDeskWebApi.Requests.User.OrganizationAdmin.RevokeSupportAgentInviteCode;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationAdminController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public OrganizationAdminController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [Authorize(Roles = "OrganizationAdmin")]
    [HttpGet("supportAgents")]
    public async Task<ActionResult<GetSupportAgentsResult>> GetSupportAgents(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetSupportAgentsRequest(), cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = "OrganizationAdmin")]
    [HttpGet("dashboard")]
    public async Task<ActionResult<GetDashboardResult>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetDashboardRequest(), cancellationToken);

        return Ok(result);
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPost("generateInviteCode")]
    public async Task<ActionResult<GenerateSupportAgentInviteCodeResult>> GenerateSupportAgentInviteCode(GenerateSupportAgentInviteCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPut("revokeInviteCode")]
    public async Task<ActionResult> RevokeSupportAgentInviteCode(RevokeSupportAgentInviteCodeRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
}
