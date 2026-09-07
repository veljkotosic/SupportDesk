using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary.Dtos;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;

internal sealed class GetOrganizationSupportAgentsSummaryQueryHandler
    : AbstractQueryHandler<GetOrganizationSupportAgentsSummaryQuery, GetOrganizationSupportAgentsSummaryQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationSupportAgentsSummaryQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationSupportAgentsSummaryQueryResult> ExecuteAsync(GetOrganizationSupportAgentsSummaryQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId((Guid)_tenantContext.GetCurrentOrganizationId()!);

        var agents = await _applicationDbContext.DomainUsers
            .AsNoTracking()
            .Where(user => user.OrganizationId == organizationId && user.Role == UserRole.SupportAgent)
            .OrderBy(user => user.UserName)
            .Select(user => new SupportAgentSummary(
                user.Id.IdValue,
                user.UserName.UserNameValue,
                user.Email.EmailValue,
                _applicationDbContext.Tickets.Count(ticket => ticket.SupportAgentId == user.Id && ticket.Status == TicketStatus.Assigned),
                _applicationDbContext.Tickets.Count(ticket => ticket.SupportAgentId == user.Id && ticket.Status == TicketStatus.Closed),
                user.CreatedAt.CreatedAtValue))
            .ToListAsync(cancellationToken);

        return new GetOrganizationSupportAgentsSummaryQueryResult(agents);
    }
}