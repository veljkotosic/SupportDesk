using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.Faq.AddFaq;
using SupportDeskWebApi.Requests.Faq.RemoveFaq;
using SupportDeskWebApi.Requests.Faq.UpdateFaq;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaqController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public FaqController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPost]
    public async Task<ActionResult<AddFaqResult>> AddFaq(AddFaqRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPut]
    public async Task<ActionResult> UpdateFaq(UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);

        return Ok();
    }

    [Authorize(Roles = "OrganizationAdmin")]
    [HttpDelete]
    public async Task<ActionResult> RemoveFaq(RemoveFaqRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
}
