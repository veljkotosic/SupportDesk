using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Domain.Abstract.Validation;

namespace SupportDesk.WebApi.ExceptionHandlers;

internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ValidationExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }
        
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Validation Error.",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            }
        };

        var errors = validationException.ValidationErrors
            .ToDictionary(e => e.Code, e => e.Message);
        
        context.ProblemDetails.Extensions.Add("errors", errors);
        
        return await _problemDetailsService.TryWriteAsync(context);
    }
}