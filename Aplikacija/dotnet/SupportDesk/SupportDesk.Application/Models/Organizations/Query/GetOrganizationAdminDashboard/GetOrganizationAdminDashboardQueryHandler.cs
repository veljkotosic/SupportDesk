using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Abstract.ReadModels;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

internal sealed class GetOrganizationAdminDashboardQueryHandler
    : AbstractQueryHandler<GetOrganizationAdminDashboardQuery, GetOrganizationAdminDashboardQueryResult>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly ITicketReadModelService _ticketReadModelService;

    public GetOrganizationAdminDashboardQueryHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        ITicketReadModelService ticketReadModelService)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _ticketReadModelService = ticketReadModelService;
    }

    protected override async Task<GetOrganizationAdminDashboardQueryResult> ExecuteAsync(
        GetOrganizationAdminDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        return await _ticketReadModelService.GetOrganizationAdminDashboardAsync(
            query,
            organizationId,
            _timeProvider,
            cancellationToken);
    }
}
