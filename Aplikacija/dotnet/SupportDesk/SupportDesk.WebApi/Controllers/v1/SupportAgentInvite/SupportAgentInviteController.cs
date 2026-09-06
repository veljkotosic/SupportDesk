using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;
using SupportDesk.Application.Models.SupportAgentInvites.Command.RevokeSupportAgentInvite;
using SupportDesk.WebApi.Controllers.v1.SupportAgentInvite.Requests;

namespace SupportDesk.WebApi.Controllers.v1.SupportAgentInvite;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SupportAgentInviteController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public SupportAgentInviteController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateSupportAgentInviteCommandResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateSupportAgentInviteCommandResult>> CreateSupportAgentInvite(
        [FromBody] CreateSupportAgentInviteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSupportAgentInviteCommand(request.Email);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);      
    }

    [Authorize]
    [HttpDelete("{supportAgentInviteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeSupportAgentInvite(
        [FromRoute] Guid supportAgentInviteId,
        CancellationToken cancellationToken)
    {
        var command = new RevokeSupportAgentInviteCommand(supportAgentInviteId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();     
    }
}