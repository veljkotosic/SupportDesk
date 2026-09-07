using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

namespace SupportDesk.WebApi.Controllers.v1.Organization;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class OrganizationController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;

    public OrganizationController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    [Authorize]
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(GetOrganizationAdminDashboardQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrganizationAdminDashboardQueryResult>> GetOrganizationAdminDashboard(CancellationToken cancellationToken)
    {
        var query = new GetOrganizationAdminDashboardQuery();

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
}