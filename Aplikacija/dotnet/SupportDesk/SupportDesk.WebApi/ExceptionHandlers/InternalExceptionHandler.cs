using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SupportDesk.WebApi.ExceptionHandlers;

internal sealed class InternalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<InternalExceptionHandler> _logger;

    public InternalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<InternalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "Something went wrong."
            }
        };
        
        _logger.LogError(exception, "Internal Server Error");

        return await _problemDetailsService.TryWriteAsync(context);
    }
}