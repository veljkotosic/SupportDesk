using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicket;

internal sealed class GetTicketQueryHandler
    : AbstractQueryHandler<GetTicketQuery, GetTicketQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;

    public GetTicketQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITenantContext tenantContext,
        IApplicationDbContext applicationDbContext)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
        _applicationDbContext = applicationDbContext;
    }

    protected override async Task<GetTicketQueryResult> ExecuteAsync(GetTicketQuery query, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(query.TicketId);
        var currentUserId = new UserId(_userContext.GetCurrentUserId());
        var organizationId = _tenantContext.GetCurrentOrganizationId();

        var ticketDetails = await (
            from ticket in _applicationDbContext.Tickets.IgnoreQueryFilters().AsNoTracking()
            where ticket.Id == ticketId
            
            join organization in _applicationDbContext.Organizations.IgnoreQueryFilters().AsNoTracking()
                on ticket.OrganizationId equals organization.Id
                
            join category in _applicationDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
                on ticket.CategoryId equals category.Id
                
            join customer in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.CustomerId equals customer.Id
                
            join agent in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.SupportAgentId equals agent.Id into agentGroup
            from agent in agentGroup.DefaultIfEmpty()
            
            select new DashboardTicketDetailsDto(
                ticket.Id.IdValue,
                organization.Id.IdValue,
                organization.Name.NameValue,
                category.Id.IdValue,
                category.Name.NameValue,
                customer.Id.IdValue,
                customer.UserName.UserNameValue,
                agent != null ? agent.Id.IdValue : null,
                agent != null ? agent.UserName.UserNameValue : null,
                ticket.Status,
                ticket.Priority,
                ticket.Subject.SubjectValue,
                ticket.OpenedAt.OpenedAtValue,
                ticket.AssignedAt != null ? ticket.AssignedAt.AssignedAtValue : null,
                ticket.ClosedAt != null ? ticket.ClosedAt.ClosedAtValue : null,
                ticket.Feedback,
                ticket.LastMessageAt != null ? ticket.LastMessageAt.LastMessageAtValue : null,
                _applicationDbContext.TicketNotifications
                    .AsNoTracking()
                    .Where(notification => notification.TicketId == ticket.Id && notification.Status == TicketNotificationStatus.Unread)
                    .Select(notification => new TicketNotificationDetailsDto(
                        notification.Id.IdValue,
                        notification.OrganizationId.IdValue,
                        notification.TicketId.IdValue,
                        notification.Text.TextValue,
                        notification.Status,
                        notification.CreatedAt.CreatedAtValue))
                    .ToList()
            )
        ).FirstOrDefaultAsync(cancellationToken);

        if (ticketDetails is null || (organizationId is null && ticketDetails.CustomerId != currentUserId.IdValue))
        {
            throw new ValidationException(TicketErrors.NotFound(ticketId));
        }

        return new GetTicketQueryResult(ticketDetails);
    }
}