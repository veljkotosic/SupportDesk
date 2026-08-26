using Microsoft.AspNetCore.Http;
using SupportDesk.Application.Abstract.Auth;

namespace SupportDesk.Infrastructure.Auth.TenantContext;

public class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetCurrentOrganizationId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        var organizationIdClaim = user?.FindFirst("organizationId");
        
        if (organizationIdClaim is null)
        {
            return null;
        }
        
        return Guid.Parse(organizationIdClaim.Value);
    }
}