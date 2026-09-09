using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Messages.Command.SendMessage;
using SupportDesk.Application.Models.Messages.Query.GetMessageDetails;
using SupportDesk.WebApi.Controllers.v1.Message.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Message;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class MessageController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher  _queryDispatcher;

    public MessageController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(SendMessageCommandResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SendMessageCommandResult>> SendMessage(
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SendMessageCommand(request.TicketId, request.Text);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize]
    [HttpGet("{messageId:guid}")]
    [ProducesResponseType(typeof(GetMessageDetailsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetMessageDetailsQueryResult>> GetMessage(
        [FromRoute] Guid messageId,
        CancellationToken cancellationToken)
    {
        var query = new GetMessageDetailsQuery(messageId);
        
        var result = await  _queryDispatcher.DispatchAsync(query, cancellationToken);
        
        return Ok(result);
    }
}