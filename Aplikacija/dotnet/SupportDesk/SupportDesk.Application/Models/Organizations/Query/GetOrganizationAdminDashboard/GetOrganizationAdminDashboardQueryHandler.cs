using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

internal sealed class GetOrganizationAdminDashboardQueryHandler
    : AbstractQueryHandler<GetOrganizationAdminDashboardQuery, GetOrganizationAdminDashboardQueryResult>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetOrganizationAdminDashboardQueryHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext, 
        IApplicationDbContext applicationDbContext)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationAdminDashboardQueryResult> ExecuteAsync(GetOrganizationAdminDashboardQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId((Guid)_tenantContext.GetCurrentOrganizationId()!);
        
        var organizationName = await _applicationDbContext.Organizations
            .Where(organization => organization.Id == organizationId)
            .Select(organization => organization.Name)
            .FirstAsync(cancellationToken);
        
        var supportAgentsQuery = _applicationDbContext.DomainUsers
            .Where(user => user.OrganizationId == organizationId && user.Role == UserRole.SupportAgent);

        var openTickets = await _applicationDbContext.Tickets.CountAsync(ticket => ticket.Status == TicketStatus.Open, cancellationToken);
        var assignedTickets = await _applicationDbContext.Tickets.CountAsync(ticket => ticket.Status == TicketStatus.Assigned, cancellationToken);
        var closedTickets = await _applicationDbContext.Tickets.CountAsync(ticket => ticket.Status == TicketStatus.Closed, cancellationToken);
        var agentCount = await supportAgentsQuery.CountAsync(cancellationToken);

        var summary = new OrganizationAdminDashboardSummaryDto(openTickets, assignedTickets, closedTickets, agentCount);
        
        var agents = await supportAgentsQuery
            .AsNoTracking()
            .OrderBy(supportAgent => supportAgent.UserName)
            .Select(supportAgent => new OrganizationAdminDashboardAgentDto(
                supportAgent.Id.IdValue,
                supportAgent.UserName.UserNameValue,
                _applicationDbContext.Tickets.Count(ticket => 
                    ticket.SupportAgentId == supportAgent.Id && 
                    ticket.Status == TicketStatus.Assigned)))
            .ToListAsync(cancellationToken);
        
        var recentTickets = await (
            from ticket in _applicationDbContext.Tickets.AsNoTracking()
            
            join organization in _applicationDbContext.Organizations.AsNoTracking()
                on ticket.OrganizationId equals organization.Id
                
            join category in _applicationDbContext.Categories.AsNoTracking()
                on ticket.CategoryId equals category.Id
                
            join customer in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.CustomerId equals customer.Id
                
            join supportAgent in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.SupportAgentId equals supportAgent.Id into agentGroup
            from supportAgent in agentGroup.DefaultIfEmpty()

            orderby ticket.LastMessageAt descending

            select new DashboardTicketDetailsDto(
                ticket.Id.IdValue,
                organization.Id.IdValue,
                organization.Name.NameValue,
                category.Id.IdValue,
                category.Name.NameValue,
                customer.Id.IdValue,
                customer.UserName.UserNameValue,
                supportAgent == null ? null : supportAgent.Id.IdValue,
                supportAgent == null ? null : supportAgent.UserName.UserNameValue,
                ticket.Status,
                ticket.Priority,
                ticket.Subject.SubjectValue,
                ticket.OpenedAt.OpenedAtValue,
                ticket.AssignedAt == null ? null : ticket.AssignedAt.AssignedAtValue,
                ticket.ClosedAt == null ? null : ticket.ClosedAt.ClosedAtValue,
                ticket.Feedback,
                ticket.LastMessageAt == null ? null : ticket.LastMessageAt.LastMessageAtValue,
                
                (from notification in _applicationDbContext.TicketNotifications.AsNoTracking()
                 where notification.TicketId == ticket.Id && notification.Status == TicketNotificationStatus.Unread
                 select new TicketNotificationDetailsDto(
                     notification.Id.IdValue,
                     notification.OrganizationId.IdValue,
                     notification.TicketId.IdValue,
                     notification.Text.TextValue,
                     notification.Status,
                     notification.CreatedAt.CreatedAtValue)
                ).ToList()
            )
        )
        .Take(6)
        .ToListAsync(cancellationToken);

        var startDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-6);

        var startOpenedAt = new TicketOpenedAt(startDate);
        var startClosedAt = new TicketClosedAt(startDate);

        var volumeTickets = await _applicationDbContext.Tickets
            .AsNoTracking()
            .Where(t => t.OpenedAt >= startOpenedAt || (t.ClosedAt != null && t.ClosedAt >= startClosedAt))
            .Select(t => new
            {
                OpenedAt = t.OpenedAt.OpenedAtValue,
                ClosedAt = t.ClosedAt == null ? (DateTime?)null : t.ClosedAt.ClosedAtValue,
                t.Status
            })
            .ToListAsync(cancellationToken);
        
        var ticketVolume = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                var dateOnly = DateOnly.FromDateTime(date);
                return new OrganizationAdminDashboardTicketVolumeEntryDto(
                    dateOnly,
                    volumeTickets.Count(t => DateOnly.FromDateTime(t.OpenedAt) == dateOnly),
                    volumeTickets.Count(t => t.Status == TicketStatus.Closed && 
                                             t.ClosedAt.HasValue && 
                                             DateOnly.FromDateTime(t.ClosedAt.Value) == dateOnly));
            })
            .ToList();
        
        return new GetOrganizationAdminDashboardQueryResult(organizationName.NameValue, summary, ticketVolume, agents, recentTickets);
    }
}