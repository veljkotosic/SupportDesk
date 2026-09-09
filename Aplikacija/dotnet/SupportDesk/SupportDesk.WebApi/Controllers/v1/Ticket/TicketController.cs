using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Tickets.Command.AssignTicket;
using SupportDesk.Application.Models.Tickets.Command.CloseTicket;
using SupportDesk.Application.Models.Tickets.Command.GiveFeedback;
using SupportDesk.Application.Models.Tickets.Command.OpenTicket;
using SupportDesk.Application.Models.Tickets.Command.ReadAllNotifications;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;
using SupportDesk.Application.Models.Tickets.Query.GetTicket;
using SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo;
using SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Ticket;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TicketController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public TicketController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
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
    
    [Authorize]
    [HttpPatch("{ticketId:guid}/readAllNotifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReadAllNotifications(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var command = new ReadAllNotificationsCommand(ticketId);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();     
    }
    
    [Authorize]
    [HttpGet("customer")]
    [ProducesResponseType(typeof(GetCustomerTicketsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetCustomerTicketsQueryResult>> GetCustomerTickets(
        [FromQuery] GetCustomerTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
       
        return Ok(result);
    }

    [Authorize]
    [HttpGet("organization")]
    [ProducesResponseType(typeof(GetOrganizationTicketsQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOrganizationTicketsQueryResult>> GetOrganizationTickets(
        [FromQuery] GetOrganizationTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(query, cancellationToken);
       
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(GetTicketQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetTicketQueryResult>> GetTicket(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(new GetTicketQuery(ticketId), cancellationToken);
        
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{ticketId:guid}/info")]
    [ProducesResponseType(typeof(GetTicketViewInfoQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetTicketViewInfoQueryResult>> GetTicketInfo(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var result = await _queryDispatcher.DispatchAsync(new GetTicketViewInfoQuery(ticketId), cancellationToken);
        
        return Ok(result);
    }
}