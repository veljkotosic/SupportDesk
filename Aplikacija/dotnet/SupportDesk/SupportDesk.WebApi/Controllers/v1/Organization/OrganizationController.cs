using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers;

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
    
    [Authorize]
    [HttpGet("knowledgeBase")]
    [ProducesResponseType(typeof(GetOrganizationKnowledgeBaseQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrganizationKnowledgeBaseQueryResult>> GetOrganizationKnowledgeBase(CancellationToken cancellationToken)
    {
        var query = new GetOrganizationKnowledgeBaseQuery();

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("supportAgentsSummary")]
    [ProducesResponseType(typeof(GetOrganizationSupportAgentsSummaryQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrganizationSupportAgentsSummaryQueryResult>> GetOrganizationSupportAgentsSummary(CancellationToken cancellationToken)
    {
        var query = new GetOrganizationSupportAgentsSummaryQuery();

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("all")]
    [ProducesResponseType(typeof(GetAllOrganizationsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAllOrganizationsQueryResult>> GetAllOrganizations(CancellationToken cancellationToken)
    {
        var query = new GetAllOrganizationsQuery();

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{organizationId:guid}/categories")]
    [ProducesResponseType(typeof(GetOrganizationCategoriesQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetOrganizationCategoriesQueryResult>> GetAllOrganizationCategories(
        [FromRoute] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationCategoriesQuery(organizationId);

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{organizationId:guid}/faqs")]
    [ProducesResponseType(typeof(GetOrganizationFaqsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetOrganizationFaqsQueryResult>> GetAllOrganizationFaqs(
        [FromRoute] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationFaqsQuery(organizationId);

        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("templateAnswers")]
    [ProducesResponseType(typeof(GetOrganizationTemplateAnswersQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]   
    public async Task<ActionResult<GetOrganizationTemplateAnswersQueryResult>> GetOrganizationTemplateAnswers(CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(new GetOrganizationTemplateAnswersQuery(), cancellationToken);
        
        return Ok(result);      
    }
}