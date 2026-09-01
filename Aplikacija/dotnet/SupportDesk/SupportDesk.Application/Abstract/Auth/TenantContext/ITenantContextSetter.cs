namespace SupportDesk.Application.Abstract.Auth.TenantContext;

/// <summary>
/// Allows non-HTTP execution pipelines (such as background workers, message consumers, and test harnesses)
/// to explicitly set the active tenant/organization context for the current dependency injection scope.
/// </summary>
public interface ITenantContextSetter
{
    /// <summary>
    /// Sets the unique identifier of the organization/tenant for the current scoped operation.
    /// </summary>
    /// <param name="organizationId">The organization identifier to scope to, or <see langword="null"/> if the operation is tenant-agnostic.</param>
    void SetCurrentOrganizationId(Guid? organizationId);
}