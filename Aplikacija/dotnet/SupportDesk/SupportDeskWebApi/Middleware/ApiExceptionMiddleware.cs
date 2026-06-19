using System.Net;
using SupportDeskWebApi.Auth.AuthService;

namespace SupportDeskWebApi.Middleware;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApiExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = GetStatusCode(exception);
            var message = statusCode == HttpStatusCode.InternalServerError
                ? "Something went wrong."
                : exception.Message;

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = context.Response.StatusCode,
                message,
                errors = exception is AuthException { Errors.Count: > 0 } authException
                    ? authException.Errors.Select(error => new { message = error })
                    : null,
            });
        }
    }

    private static HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            AuthException => HttpStatusCode.BadRequest,
            InvalidOperationException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.BadRequest,
        };
    }
}
