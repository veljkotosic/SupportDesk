using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.TemplateAnswers.Command.AddTemplateAnswer;
using SupportDesk.Application.Models.TemplateAnswers.Command.DeleteTemplateAnswer;
using SupportDesk.Application.Models.TemplateAnswers.Command.UpdateTemplateAnswerDetails;
using SupportDesk.WebApi.Controllers.v1.TemplateAnswer.Requests;

namespace SupportDesk.WebApi.Controllers.v1.TemplateAnswer;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TemplateAnswerController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public TemplateAnswerController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddTemplateAnswerCommandResult>> AddTemplateAnswer(
        [FromBody] AddTemplateAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTemplateAnswerCommand(request.Title, request.Text);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);       
    }

    [Authorize]
    [HttpDelete("{templateAnswerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTemplateAnswer(
        [FromRoute] Guid templateAnswerId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTemplateAnswerCommand(templateAnswerId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();       
    }

    [Authorize]
    [HttpPatch("{templateAnswerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTemplateAnswerDetails(
        [FromRoute] Guid templateAnswerId,
        [FromBody] UpdateTemplateAnswerDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTemplateAnswerDetailsCommand(templateAnswerId, request.Title, request.Text);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();      
    }
}