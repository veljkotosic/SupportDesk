using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;

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
    
    
}