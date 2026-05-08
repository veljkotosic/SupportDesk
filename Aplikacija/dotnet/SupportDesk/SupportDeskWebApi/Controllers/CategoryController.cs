using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Category.AddCategory;
using SupportDeskWebApi.Requests.Category.RemoveCategory;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public CategoryController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPost("addCategory")]
    public async Task<ActionResult<AddCategoryResult>> AddCategory(AddCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpDelete("removeCategory")]
    public async Task<ActionResult> RemoveCategory(RemoveCategoryRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();   
    }
}