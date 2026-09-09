using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo;

internal sealed class GetTicketViewInfoQueryHandler
    : AbstractQueryHandler<GetTicketViewInfoQuery, GetTicketViewInfoQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    private readonly IApplicationDbContext _applicationDbContext;

    public GetTicketViewInfoQueryHandler(
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

    protected override async Task<GetTicketViewInfoQueryResult> ExecuteAsync(
        GetTicketViewInfoQuery query, 
        CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(query.TicketId);
        var currentUserId = new UserId(_userContext.GetCurrentUserId());
        var organizationId = _tenantContext.GetCurrentOrganizationId();
        var isOrganizationMember = organizationId != null;

        var ticketHeader = await (
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
            
            select new
            {
                Id = ticket.Id.IdValue,
                OrganizationId = organization.Id.IdValue,
                OrganizationName = organization.Name.NameValue,
                CategoryId = category.Id.IdValue,
                CategoryName = category.Name.NameValue,
                CustomerId = customer.Id.IdValue,
                CustomerUserName = customer.UserName.UserNameValue,
                CustomerEmail = customer.Email.EmailValue,
                SupportAgentId = agent != null ? agent.Id.IdValue : (Guid?)null,
                SupportAgentUserName = agent != null ? agent.UserName.UserNameValue : null,
                ticket.Status,
                ticket.Priority,
                ticket.Feedback,
                Subject = ticket.Subject.SubjectValue,
                OpenedAt = ticket.OpenedAt.OpenedAtValue,
                AssignedAt = ticket.AssignedAt != null ? ticket.AssignedAt.AssignedAtValue : (DateTime?)null,
                ClosedAt = ticket.ClosedAt != null ? ticket.ClosedAt.ClosedAtValue : (DateTime?)null,
                LastMessageAt = ticket.LastMessageAt != null ? ticket.LastMessageAt.LastMessageAtValue : (DateTime?)null
            }
        ).FirstOrDefaultAsync(cancellationToken);

        if (ticketHeader is null || (!isOrganizationMember && ticketHeader.CustomerId != currentUserId.IdValue))
        {
            throw new ValidationException(TicketErrors.NotFound(ticketId));
        }

        var messages = await (
            from m in _applicationDbContext.Messages.IgnoreQueryFilters().AsNoTracking()
            where m.TicketId == ticketId
            
            join sender in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on m.SenderId equals sender.Id
                
            orderby m.CreatedAt
            
            select new TicketMessageDetailsDto(
                m.Id.IdValue,
                m.SenderId.IdValue,
                sender.UserName.UserNameValue,
                m.Text.TextValue,
                m.CreatedAt.CreatedAtValue)
        ).ToListAsync(cancellationToken);

        var notes = isOrganizationMember
            ? await (
                from n in _applicationDbContext.Notes.IgnoreQueryFilters().AsNoTracking()
                where n.TicketId == ticketId
                join author in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                    on n.AuthorId equals author.Id
                orderby n.CreatedAt
                select new TicketNoteDetailsDto(
                    n.Id.IdValue,
                    n.AuthorId.IdValue,
                    author.UserName.UserNameValue,
                    n.Text.TextValue,
                    n.CreatedAt.CreatedAtValue)
            ).ToListAsync(cancellationToken)
            : [];

        var ticketViewInfo = new TicketViewInfoDto(
            ticketHeader.Id,
            ticketHeader.OrganizationId,
            ticketHeader.OrganizationName,
            ticketHeader.CategoryId,
            ticketHeader.CategoryName,
            ticketHeader.CustomerId,
            ticketHeader.CustomerUserName,
            ticketHeader.CustomerEmail,
            ticketHeader.SupportAgentId,
            ticketHeader.SupportAgentUserName,
            ticketHeader.Status,
            ticketHeader.Priority,
            ticketHeader.Feedback,
            ticketHeader.Subject,
            ticketHeader.OpenedAt,
            ticketHeader.AssignedAt,
            ticketHeader.ClosedAt,
            ticketHeader.LastMessageAt,
            messages,
            notes);

        return new GetTicketViewInfoQueryResult(ticketViewInfo);
    }
}