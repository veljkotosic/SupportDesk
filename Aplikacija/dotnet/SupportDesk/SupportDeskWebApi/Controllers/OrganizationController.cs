using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Organization.ListCategories;
using SupportDeskWebApi.Requests.Organization.ListOrganizations;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public OrganizationController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("listAll")]
    public async Task<ActionResult<ListOrganizationsResult>> ListOrganizations(CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new ListOrganizationsRequest(), cancellationToken);
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{organizationId:guid}/categories")]
    public async Task<ActionResult<ListCategoriesResult>> ListCategories([FromRoute] Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new ListCategoriesRequest(organizationId), cancellationToken);
        return Ok(result);
    }
}