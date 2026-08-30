using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Faqs.Command.AddFaq;
using SupportDesk.Application.Models.Faqs.Command.DeleteFaq;
using SupportDesk.Application.Models.Faqs.Command.UpdateFaqDetails;
using SupportDesk.WebApi.Controllers.v1.Faq.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Faq;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class FaqController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public FaqController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddFaqCommandResult>> AddFaq(
        [FromBody] AddFaqRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddFaqCommand(request.Question, request.Answer);

        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);       
    }

    [Authorize]
    [HttpDelete("{faqId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFaq(
        [FromRoute] Guid faqId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFaqCommand(faqId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPatch("{faqId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateFaqDetails(
        [FromRoute] Guid faqId,
        [FromBody] UpdateFaqDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFaqDetailsCommand(faqId, request.Question, request.Answer);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();       
    }
}