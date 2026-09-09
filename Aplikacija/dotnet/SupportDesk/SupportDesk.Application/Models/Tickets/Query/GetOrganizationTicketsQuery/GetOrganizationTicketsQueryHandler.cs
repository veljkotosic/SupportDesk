using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Enums;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

internal sealed class GetOrganizationTicketsQueryHandler
    : AbstractQueryHandler<GetOrganizationTicketsQuery, GetOrganizationTicketsQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;

    public GetOrganizationTicketsQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IApplicationDbContext applicationDbContext)
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetOrganizationTicketsQueryResult> ExecuteAsync(
        GetOrganizationTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId((Guid)_tenantContext.GetCurrentOrganizationId()!);

        var orgTickets = _applicationDbContext.Tickets
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId);

        var allCount = await orgTickets.CountAsync(cancellationToken);
        var openCount = await orgTickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var assignedCount = await orgTickets.CountAsync(t => t.Status == TicketStatus.Assigned, cancellationToken);
        var closedCount = await orgTickets.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);

        var projectedQuery =
            from ticket in orgTickets
            
            join category in _applicationDbContext.Categories.AsNoTracking()
                on ticket.CategoryId equals category.Id
                
            join customer in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.CustomerId equals customer.Id
                
            join agent in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.SupportAgentId equals agent.Id into agentGroup
            from agent in agentGroup.DefaultIfEmpty()
            
            select new
            {
                Ticket = ticket,
                CategoryName = category.Name.NameValue,
                CustomerUserName = customer.UserName.UserNameValue,
                CustomerEmail = customer.Email.EmailValue,
                SupportAgentUserName = agent != null ? agent.UserName.UserNameValue : null
            };

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim().ToLower();
            var isGuid = Guid.TryParse(query.SearchTerm.Trim(), out var parsedGuid);
            var parsedTicketId = isGuid ? new TicketId(parsedGuid) : null;

            projectedQuery = projectedQuery.Where(item =>
                (parsedTicketId != null && item.Ticket.Id == parsedTicketId) ||
                item.Ticket.Subject.SubjectValue.ToLower().Contains(search) ||
                item.CustomerUserName.ToLower().Contains(search) ||
                item.CategoryName.ToLower().Contains(search));
        }

        if (query.Status.HasValue)
        {
            projectedQuery = projectedQuery.Where(item => item.Ticket.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            projectedQuery = projectedQuery.Where(item => item.Ticket.Priority == query.Priority.Value);
        }

        var totalCount = await projectedQuery.CountAsync(cancellationToken);

        var orderedQuery = query.SortBy switch
        {
            TicketSortOption.Oldest => projectedQuery
                .OrderBy(item => item.Ticket.LastMessageAt)
                .ThenBy(item => item.Ticket.Id),

            TicketSortOption.Priority => projectedQuery
                .OrderByDescending(item => item.Ticket.Priority)
                .ThenByDescending(item => item.Ticket.LastMessageAt)
                .ThenByDescending(item => item.Ticket.Id),

            TicketSortOption.Latest or _ => projectedQuery
                .OrderByDescending(item => item.Ticket.LastMessageAt)
                .ThenByDescending(item => item.Ticket.Id)
        };

        var tickets = await orderedQuery
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(item => new OrganizationTicketListingDto(
                item.Ticket.Id.IdValue,
                item.Ticket.CategoryId.IdValue,
                item.CategoryName,
                item.Ticket.CustomerId.IdValue,
                item.CustomerUserName,
                item.CustomerEmail,
                item.Ticket.SupportAgentId != null ? item.Ticket.SupportAgentId.IdValue : null,
                item.SupportAgentUserName,
                item.Ticket.Status,
                item.Ticket.Priority,
                item.Ticket.Feedback,
                item.Ticket.Subject.SubjectValue,
                item.Ticket.LastMessageAt != null ? item.Ticket.LastMessageAt.LastMessageAtValue : null
            ))
            .ToListAsync(cancellationToken);

        return new GetOrganizationTicketsQueryResult(
            tickets,
            totalCount,
            allCount,
            openCount,
            assignedCount,
            closedCount);
    }
}