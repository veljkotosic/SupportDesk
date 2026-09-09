using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Tickets.Event.TicketOpened;

internal sealed class PublishTicketOpenedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<TicketOpenedDomainEvent>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishTicketOpenedRealtimeUpdateDomainEventHandler(
        IApplicationDbContext applicationDbContext,
        IRealtimePublisher realtimePublisher)
    {
        _applicationDbContext = applicationDbContext;
        _realtimePublisher = realtimePublisher;
    }

    public async Task HandleAsync(TicketOpenedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var ticketDetails = await GetQuery(domainEvent.TicketId).FirstOrDefaultAsync(cancellationToken);

        if (ticketDetails is null)
        {
            throw new ValidationException(TicketErrors.NotFound(domainEvent.TicketId));
        }
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.OrganizationDashboard,
            ticketDetails.OrganizationId.ToString(),
            "TicketOpened",
            ticketDetails,
            cancellationToken);
    }
    
    private IQueryable<DashboardTicketDetailsDto> GetQuery(TicketId ticketId)
    {
        var queryable =
            from ticket in _applicationDbContext.Tickets.IgnoreQueryFilters().AsNoTracking()
            where ticket.Id == ticketId
            
            join organization in _applicationDbContext.Organizations.IgnoreQueryFilters().AsNoTracking()
                on ticket.OrganizationId equals organization.Id

            join category in _applicationDbContext.Categories.IgnoreQueryFilters().AsNoTracking()
                on ticket.CategoryId equals category.Id

            join customer in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.CustomerId equals customer.Id

            join supportAgent in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                on ticket.SupportAgentId equals supportAgent.Id into agentGroup
            from supportAgent in agentGroup.DefaultIfEmpty()

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
                (from notification in _applicationDbContext.TicketNotifications.IgnoreQueryFilters().AsNoTracking()
                    where notification.TicketId == ticketId && notification.Status == TicketNotificationStatus.Unread
                    
                    select new TicketNotificationDetailsDto(
                        notification.Id.IdValue,
                        notification.OrganizationId.IdValue,
                        notification.TicketId.IdValue,
                        notification.Text.TextValue,
                        notification.Status,
                        notification.CreatedAt.CreatedAtValue)
                ).ToList()
            );
        
        return queryable;
    }   
}