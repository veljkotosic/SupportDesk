using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Message.SendMessage;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public MessageController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [Authorize(Roles = "Customer, SupportAgent")]
    [HttpPost]
    public async Task<ActionResult<SendMessageResult>> SendMessage(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
}