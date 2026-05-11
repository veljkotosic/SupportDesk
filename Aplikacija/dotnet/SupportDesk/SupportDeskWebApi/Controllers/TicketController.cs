using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Ticket.AssignTicket;
using SupportDeskWebApi.Requests.Ticket.CloseTicket;
using SupportDeskWebApi.Requests.Ticket.CreateTicket;
using SupportDeskWebApi.Requests.Ticket.GiveFeedback;

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
}