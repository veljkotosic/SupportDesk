using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Abstract.ReadModels;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

internal sealed class GetOrganizationTicketsQueryHandler
    : AbstractQueryHandler<GetOrganizationTicketsQuery, GetOrganizationTicketsQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly ITicketReadModelService _ticketReadModelService;

    public GetOrganizationTicketsQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        ITicketReadModelService ticketReadModelService)
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _ticketReadModelService = ticketReadModelService;
    }

    protected override async Task<GetOrganizationTicketsQueryResult> ExecuteAsync(
        GetOrganizationTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        return await _ticketReadModelService.GetOrganizationTicketsAsync(
            query,
            organizationId,
            cancellationToken);
    }
}