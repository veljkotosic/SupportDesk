using SupportDesk.Application.Abstract.Auth.TenantContext;

namespace SupportDesk.Infrastructure.Auth.TenantContext;

public sealed class WorkerTenantContext : ITenantContext, ITenantContextSetter
{
    private Guid? _organizationId = null;
    
    public Guid? GetCurrentOrganizationId()
    {
        return _organizationId;
    }

    public void SetCurrentOrganizationId(Guid? organizationId)
    {
        _organizationId = organizationId;
    }
}