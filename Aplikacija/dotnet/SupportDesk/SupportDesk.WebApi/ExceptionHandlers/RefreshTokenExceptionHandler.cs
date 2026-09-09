using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Infrastructure.Persistence.RefreshToken;

namespace SupportDesk.WebApi.ExceptionHandlers;

public sealed class RefreshTokenExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public RefreshTokenExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RefreshTokenException refreshTokenException)
        {
            return false;
        }
        
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "Authentication Error.",
                Detail = "Refresh token error.",
                Status = StatusCodes.Status401Unauthorized,
            }
        };
        
        var errors = new Dictionary<string, string>
        {
            [refreshTokenException.ErrorCode] = refreshTokenException.ErrorMessage
        };
        context.ProblemDetails.Extensions.Add("errors", errors);
        
        context.HttpContext.Response.Cookies.Delete("accessToken");
        context.HttpContext.Response.Cookies.Delete("refreshToken");
        
        return await _problemDetailsService.TryWriteAsync(context);
    }
}