using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SupportDesk.Application.Abstract.Auth;

namespace SupportDesk.Infrastructure.Auth.UserContext;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public Guid GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user is null || !user.Identity!.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim is null)
        {
            throw new UnauthorizedAccessException("User id claim not found");
        }
        
        return Guid.Parse(userIdClaim.Value);
    }
}