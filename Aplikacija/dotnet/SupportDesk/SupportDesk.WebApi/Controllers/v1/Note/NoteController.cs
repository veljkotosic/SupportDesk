using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Notes.Command.AddNote;
using SupportDesk.WebApi.Controllers.v1.Note.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Note;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class NoteController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public NoteController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(AddNoteCommandResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddNoteCommandResult>> AddNote(
        [FromBody] AddNoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddNoteCommand(request.TicketId, request.Text);

        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return StatusCode(StatusCodes.Status201Created, result);      
    }
}