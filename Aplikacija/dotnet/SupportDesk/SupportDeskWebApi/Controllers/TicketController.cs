using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Ticket.AssignTicket;
using SupportDeskWebApi.Requests.Ticket.CloseTicket;
using SupportDeskWebApi.Requests.Ticket.CreateTicket;
using SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;
using SupportDeskWebApi.Requests.Ticket.GetOrganizationTickets;
using SupportDeskWebApi.Requests.Ticket.GetTicket;
using SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;
using SupportDeskWebApi.Requests.Ticket.GiveFeedback;
using SupportDeskWebApi.Requests.Ticket.ReadAllNotifications;
using TicketPriority = SupportDeskWebApi.Data.Entities.Ticket.Enums.TicketPriority;
using TicketStatus = SupportDeskWebApi.Data.Entities.Ticket.Enums.TicketStatus;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TicketController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<CreateTicketResult>> CreateTicket(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPut("feedback")]
    public async Task<ActionResult> GiveFeedback(GiveFeedbackRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
    
    [Authorize(Roles = "SupportAgent")]
    [HttpPut("close")]
    public async Task<ActionResult> CloseTicket(CloseTicketRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
    
    [Authorize(Roles = "SupportAgent")]
    [HttpPut("assign")]
    public async Task<ActionResult> AssignTicket(AssignTicketRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
    
    [Authorize(Roles = "Customer")]
    [HttpGet("customerTickets")]
    public async Task<ActionResult<GetCustomerTicketsResult>> GetCustomerTickets(
        [FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 50)] int take = 10,
        [FromQuery] string? search = null,
        [FromQuery] TicketStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _dispatcher.ExecuteAsync(new GetCustomerTicketsRequest(skip, take, search, status), cancellationToken);
        
        return Ok(result);
    }

    [Authorize(Roles = "OrganizationAdmin, SupportAgent")]
    [HttpGet("organizationTickets")]
    public async Task<ActionResult<GetOrganizationTicketsResult>> GetOrganizationTickets(
        [FromQuery, Range(0, int.MaxValue)] int skip = 0,
        [FromQuery, Range(1, 50)] int take = 10,
        [FromQuery] string? search = null,
        [FromQuery] TicketStatus? status = null,
        [FromQuery] TicketPriority? priority = null,
        [FromQuery] string sortBy = "latest",
        CancellationToken cancellationToken = default)
    {
        var request = new GetOrganizationTicketsRequest(skip, take, search, status, priority, sortBy);
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);

        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<GetTicketResult>> GetTicket([FromRoute] Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetTicketRequest(ticketId), cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize]
    [HttpGet("{ticketId:guid}/info")]
    public async Task<ActionResult<GetTicketViewInfoResult>> GetTicketViewInfo([FromRoute] Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(new GetTicketViewInfoRequest(ticketId), cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPut("{ticketId:guid}/readAllNotifications")]
    public async Task<ActionResult> ReadAllNotifications([FromRoute] Guid ticketId, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(new ReadAllNotificationsRequest(ticketId), cancellationToken);
        
        return Ok();
    }
}
