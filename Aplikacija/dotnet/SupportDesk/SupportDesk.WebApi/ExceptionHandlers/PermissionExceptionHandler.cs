using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.WebApi.ExceptionHandlers;

internal sealed class PermissionExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public PermissionExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not PermissionException permissionException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Authorization Error.",
                Detail = "Permission denied.",
                Status = StatusCodes.Status403Forbidden,
            }
        };

        var errors = new Dictionary<string, string>
        {
            [PermissionException.ErrorCode] = permissionException.ErrorMessage
        };
        
        context.ProblemDetails.Extensions.Add("errors", errors);

        return await _problemDetailsService.TryWriteAsync(context);
    }
}