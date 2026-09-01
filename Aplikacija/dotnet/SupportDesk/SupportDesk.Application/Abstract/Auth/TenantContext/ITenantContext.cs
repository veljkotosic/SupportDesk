namespace SupportDesk.Application.Abstract.Auth.TenantContext;

/// <summary>
/// Provides access to the current tenant context and organization identity for multi-tenant operations.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the unique identifier of the current organization/tenant, if available in the current context.
    /// </summary>
    /// <returns>
    /// The <see cref="Guid"/> representing the current organization identifier, or <see langword="null"/> if no organization is scoped.
    /// </returns>
    Guid? GetCurrentOrganizationId();
}