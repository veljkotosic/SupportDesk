using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Message.Events;
using SupportDesk.Domain.Models.Message.Validation;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Messages.Event.MessageCreated;

internal sealed class PublishMessageCreatedRealtimeUpdateDomainEventHandler
    : IDomainEventHandler<MessageCreatedDomainEvent>
{
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public PublishMessageCreatedRealtimeUpdateDomainEventHandler(
        IApplicationDbContext applicationDbContext,
        IRealtimePublisher realtimePublisher)
    {
        _applicationDbContext = applicationDbContext;
        _realtimePublisher = realtimePublisher;
    }

    public async Task HandleAsync(MessageCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var data = await GetQuery(domainEvent.MessageId).FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            throw new ValidationException(MessageErrors.NotFound(domainEvent.MessageId));
        }

        var messageDto = new RealtimeMessageDetailsDto(
            data.MessageId,
            data.SenderId,
            data.SenderUserName,
            data.Text,
            data.CreatedAt);

        await _realtimePublisher.PublishAsync(
            RealtimeHubType.Ticket,
            data.TicketId.ToString(),
            "NewMessage",
            messageDto,
            cancellationToken);

        var organizationDashboardMessageInfoDto = new OrganizationDashboardMessageInfoDto(
            data.TicketId,
            data.CreatedAt);

        await _realtimePublisher.PublishAsync(
            RealtimeHubType.OrganizationDashboard,
            data.OrganizationId.ToString(),
            "NewTicketMessage",
            organizationDashboardMessageInfoDto,
            cancellationToken);

        if (data.SenderRole == UserRole.SupportAgent)
        {
            await _realtimePublisher.PublishAsync(
                RealtimeHubType.CustomerDashboard,
                data.CustomerId.ToString(),
                "NewTicketMessage",
                messageDto,
                cancellationToken);
        }
    }

    private IQueryable<MessageRealtimeProjection> GetQuery(MessageId messageId)
    {
        return from message in _applicationDbContext.Messages.IgnoreQueryFilters().AsNoTracking()
               where message.Id == messageId
               
               join ticket in _applicationDbContext.Tickets.IgnoreQueryFilters().AsNoTracking()
                   on message.TicketId equals ticket.Id
               join sender in _applicationDbContext.DomainUsers.IgnoreQueryFilters().AsNoTracking()
                   on message.SenderId equals sender.Id
                   
               select new MessageRealtimeProjection(
                   message.Id.IdValue,
                   ticket.Id.IdValue,
                   ticket.OrganizationId.IdValue,
                   ticket.CustomerId.IdValue,
                   sender.Id.IdValue,
                   sender.UserName.UserNameValue,
                   sender.Role,
                   message.Text.TextValue,
                   message.CreatedAt.CreatedAtValue);
    }
}

internal sealed record MessageRealtimeProjection(
    Guid MessageId,
    Guid TicketId,
    Guid OrganizationId,
    Guid CustomerId,
    Guid SenderId,
    string SenderUserName,
    UserRole SenderRole,
    string Text,
    DateTime CreatedAt);

public sealed record RealtimeMessageDetailsDto(
    Guid Id,
    Guid SenderId,
    string SenderUserName,
    string Text,
    DateTime CreatedAt);

public sealed record OrganizationDashboardMessageInfoDto(
    Guid TicketId,
    DateTime CreatedAt);