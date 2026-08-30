using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Models.Categories.Command.AddCategory;
using SupportDesk.Application.Models.Categories.Command.DeleteCategory;
using SupportDesk.Application.Models.Categories.Command.UpdateCategoryDetails;
using SupportDesk.WebApi.Controllers.v1.Category.Requests;

namespace SupportDesk.WebApi.Controllers.v1.Category;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
public sealed class CategoryController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public CategoryController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddCategoryCommandResult>> AddCategory(
        [FromBody] AddCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddCategoryCommand(request.Name, request.Description);
        
        var result = await _commandDispatcher.DispatchAsync(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize]
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
    {
        await _commandDispatcher.DispatchAsync(new DeleteCategoryCommand(categoryId), cancellationToken);
        
        return NoContent();
    }

    [Authorize]
    [HttpPatch("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateCategoryDetails(
        [FromRoute] Guid categoryId,
        [FromBody] UpdateCategoryDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryDetailsCommand(categoryId, request.Name, request.Description);
        
        await _commandDispatcher.DispatchAsync(command, cancellationToken);
        
        return NoContent();
    }
    
}