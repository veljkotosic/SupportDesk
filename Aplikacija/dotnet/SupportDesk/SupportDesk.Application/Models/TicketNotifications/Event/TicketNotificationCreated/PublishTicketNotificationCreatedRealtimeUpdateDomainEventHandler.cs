using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Events;
using SupportDesk.Domain.Models.TicketNotification.Validation;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;

namespace SupportDesk.Application.Models.TicketNotifications.Event.TicketNotificationCreated;

internal sealed class PublishTicketNotificationCreatedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<TicketNotificationCreatedDomainEvent>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishTicketNotificationCreatedRealtimeUpdateDomainEventHandler(
        IApplicationDbContext applicationDbContext,
        IRealtimePublisher realtimePublisher)
    {
        _applicationDbContext = applicationDbContext;
        _realtimePublisher = realtimePublisher;
    }
    
    public async Task HandleAsync(TicketNotificationCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var ticketNotificationDetails = await GetQuery(domainEvent.TicketNotificationId).FirstOrDefaultAsync(cancellationToken);
        
        if (ticketNotificationDetails is null)
        {
            throw new ValidationException(TicketNotificationErrors.NotFound(domainEvent.TicketNotificationId));
        }
        
        await _realtimePublisher.PublishAsync(
            RealtimeHubType.CustomerDashboard,
            domainEvent.CustomerId.IdValue.ToString(),
            "NewTicketNotification",
            ticketNotificationDetails,
            cancellationToken);
    }

    private IQueryable<GetCustomerDashboardTicketNotificationDetailsQuery> GetQuery(TicketNotificationId ticketNotificationId)
    {
        var queryable =
            from ticketNotification in _applicationDbContext.TicketNotifications.AsNoTracking()
            where ticketNotification.Id == ticketNotificationId &&
                  ticketNotification.Status == TicketNotificationStatus.Unread

            select new GetCustomerDashboardTicketNotificationDetailsQuery(
                ticketNotification.Id.IdValue,
                ticketNotification.OrganizationId.IdValue,
                ticketNotification.TicketId.IdValue,
                ticketNotification.Text.TextValue,
                ticketNotification.Status,
                ticketNotification.CreatedAt.CreatedAtValue);

        return queryable;
    }
}

internal sealed record GetCustomerDashboardTicketNotificationDetailsQuery(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    string Text,
    TicketNotificationStatus Status,
    DateTime CreatedAt);