using System.Security.Claims;
using SupportDeskWebApi.Auth.Abstract;

namespace SupportDeskWebApi.Auth.UserContext;

public class HttpUserContext : IUserContext
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

    public Guid? GetCurrentUsersOrganizationId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user is null || !user.Identity!.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        var organizationIdClaim = user.FindFirst("organizationId");
        
        if (organizationIdClaim is null)
        {
            return null;
        }
        
        return Guid.Parse(organizationIdClaim.Value);
    }
}