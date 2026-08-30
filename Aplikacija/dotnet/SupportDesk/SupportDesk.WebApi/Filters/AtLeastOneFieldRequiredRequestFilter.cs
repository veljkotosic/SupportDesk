using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportDesk.WebApi.Abstract.Request;

namespace SupportDesk.WebApi.Filters;

public sealed class AtLeastOneFieldRequiredRequestFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var (_, argument) in context.ActionArguments)
        {
            if (argument is IAtLeastOneFieldRequiredRequest partialRequest && !partialRequest.HasAnyFieldProvided())
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Validation Error.",
                    Detail = "At least one field must be provided in the request body.",
                    Status = StatusCodes.Status400BadRequest
                };

                problemDetails.Extensions.Add("errors", new Dictionary<string, string>
                {
                    ["at_least_one_field_required"] = "At least one field must be provided."
                });

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        
    }
}