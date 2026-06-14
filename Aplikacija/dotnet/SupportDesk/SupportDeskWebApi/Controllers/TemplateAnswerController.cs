using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Dispatcher;
using SupportDeskWebApi.Requests.TemplateAnswer.AddTemplateAnswer;
using SupportDeskWebApi.Requests.TemplateAnswer.RemoveTemplateAnswer;
using SupportDeskWebApi.Requests.TemplateAnswer.UpdateTemplateAnswer;

namespace SupportDeskWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateAnswerController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TemplateAnswerController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPost]
    public async Task<ActionResult<AddTemplateAnswerResult>> AddTemplateAnswer(AddTemplateAnswerRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok(result);
    }
    
    [Authorize(Roles = "OrganizationAdmin")]
    [HttpPut]
    public async Task<ActionResult> UpdateTemplateAnswer(UpdateTemplateAnswerRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);

        return Ok();
    }

    [Authorize(Roles = "OrganizationAdmin")]
    [HttpDelete]
    public async Task<ActionResult> RemoveTemplateAnswer(RemoveTemplateAnswerRequest request, CancellationToken cancellationToken)
    {
        await _dispatcher.ExecuteAsync(request, cancellationToken);
        
        return Ok();
    }
}
