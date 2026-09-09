using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

internal sealed class GetCustomerTicketsQueryHandler
    : AbstractQueryHandler<GetCustomerTicketsQuery, GetCustomerTicketsQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IApplicationDbContext _applicationDbContext;
    
    public GetCustomerTicketsQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        IApplicationDbContext applicationDbContext) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetCustomerTicketsQueryResult> ExecuteAsync(GetCustomerTicketsQuery query, CancellationToken cancellationToken)
    {
        var userId = new UserId(_userContext.GetCurrentUserId());

        var customerTickets = _applicationDbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.CustomerId == userId);
        
        var allCount = await customerTickets.CountAsync(cancellationToken);
        var openCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var assignedCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Assigned, cancellationToken);
        var closedCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);
        
        var projectedQuery =
            from ticket in customerTickets
            
            join org in _applicationDbContext.Organizations.IgnoreQueryFilters().AsNoTracking()
                on ticket.OrganizationId equals org.Id
                
            join category in _applicationDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
                on ticket.CategoryId equals category.Id
                
            join customer in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.CustomerId equals customer.Id
                
            join agent in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.SupportAgentId equals agent.Id into agentGroup
            from agent in agentGroup.DefaultIfEmpty()
            
            select new
            {
                Ticket = ticket,
                OrganizationName = org.Name.NameValue,
                CategoryName = category.Name.NameValue,
                CustomerUserName = customer.UserName.UserNameValue,
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
                item.OrganizationName.ToLower().Contains(search) ||
                item.CategoryName.ToLower().Contains(search));
        }
        
        if (query.Status.HasValue)
        {
            projectedQuery = projectedQuery.Where(item => item.Ticket.Status == query.Status.Value);
        }
        
        var totalCount = await projectedQuery.CountAsync(cancellationToken);
        
        var tickets = await projectedQuery
                    .OrderByDescending(item => item.Ticket.LastMessageAt)
                    .ThenByDescending(item => item.Ticket.Id)
                    .Skip(query.Skip)
                    .Take(query.Take)
                    .Select(item => new CustomerTicketListingDto(
                        item.Ticket.Id.IdValue,
                        item.Ticket.OrganizationId.IdValue,
                        item.OrganizationName,
                        item.Ticket.CategoryId.IdValue,
                        item.CategoryName,
                        item.Ticket.CustomerId.IdValue,
                        item.CustomerUserName,
                        item.Ticket.SupportAgentId != null ? item.Ticket.SupportAgentId.IdValue : null,
                        item.SupportAgentUserName,
                        item.Ticket.Status,
                        item.Ticket.Priority,
                        item.Ticket.Feedback,
                        item.Ticket.Subject.SubjectValue,
                        item.Ticket.OpenedAt.OpenedAtValue,
                        item.Ticket.AssignedAt != null ? item.Ticket.AssignedAt.AssignedAtValue : null,
                        item.Ticket.ClosedAt != null ? item.Ticket.ClosedAt.ClosedAtValue : null,
                        item.Ticket.LastMessageAt != null ? item.Ticket.LastMessageAt.LastMessageAtValue : null,
                        _applicationDbContext.TicketNotifications
                            .AsNoTracking()
                            .Where(n => n.TicketId == item.Ticket.Id && n.Status == TicketNotificationStatus.Unread)
                            .Select(n => new TicketNotificationDetailsDto(
                                n.Id.IdValue,
                                n.OrganizationId.IdValue,
                                n.TicketId.IdValue,
                                n.Text.TextValue,
                                n.Status,
                                n.CreatedAt.CreatedAtValue))
                            .ToList()
                    ))
                    .ToListAsync(cancellationToken);
        
        return new GetCustomerTicketsQueryResult(
            tickets,
            totalCount,
            allCount,
            openCount,
            assignedCount,
            closedCount);
    }
}