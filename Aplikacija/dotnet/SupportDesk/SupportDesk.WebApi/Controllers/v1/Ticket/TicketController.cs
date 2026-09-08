using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Tickets.Command.AssignTicket;
using SupportDesk.Application.Models.Tickets.Command.CloseTicket;
using SupportDesk.Application.Models.Tickets.Command.GiveFeedback;
using SupportDesk.Application.Models.Tickets.Command.OpenTicket;
using SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Ticket;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public TicketController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(OpenTicketCommandResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OpenTicketCommandResult>> OpenTicket(
        [FromBody] OpenTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new OpenTicketCommand(
            request.OrganizationId,
            request.CategoryId,
            request.Priority,
            request.Subject,
            request.InitialMessage);

        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);      
    }

    [Authorize]
    [HttpPatch("{ticketId:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignTicket(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var command = new AssignTicketCommand(ticketId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();     
    }
    
    [Authorize]
    [HttpPatch("{ticketId:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseTicket(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var command = new CloseTicketCommand(ticketId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();     
    }
    
    [Authorize]
    [HttpPatch("{ticketId:guid}/giveFeedback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GiveFeedback(
        [FromRoute] Guid ticketId,
        [FromBody] GiveFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GiveFeedbackCommand(ticketId, request.Feedback);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();     
    }
}