using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Infrastructure.Elasticsearch.Services;

public sealed class ElasticsearchTicketReadModelService : ITicketReadModelService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchTicketReadModelService> _logger;

    public ElasticsearchTicketReadModelService(
        ElasticsearchClient client,
        ILogger<ElasticsearchTicketReadModelService> logger)
    {
        _client = client;
        _logger = logger;
    }

    #region Projections

    public async Task IndexTicketAsync(TicketReadModelDto ticket, CancellationToken cancellationToken)
    {
        var doc = new TicketDocument
        {
            Id = ticket.Id,
            OrganizationId = ticket.OrganizationId,
            OrganizationName = ticket.OrganizationName,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.CategoryName,
            CustomerId = ticket.CustomerId,
            CustomerUserName = ticket.CustomerUserName,
            CustomerEmail = ticket.CustomerEmail,
            SupportAgentId = ticket.SupportAgentId,
            SupportAgentUserName = ticket.SupportAgentUserName,
            Status = (short)ticket.Status,
            Priority = (short)ticket.Priority,
            Feedback = (short)ticket.Feedback,
            Subject = ticket.Subject,
            OpenedAt = ticket.OpenedAt,
            AssignedAt = ticket.AssignedAt,
            ClosedAt = ticket.ClosedAt,
            LastMessageAt = ticket.LastMessageAt,
            UnreadNotifications = ticket.UnreadNotifications.Select(n => new TicketNotificationDocument
            {
                Id = n.Id,
                OrganizationId = n.OrganizationId,
                TicketId = n.TicketId,
                Text = n.Text,
                Status = (short)n.Status,
                CreatedAt = n.CreatedAt
            }).ToList()
        };

        var response = await _client.IndexAsync(doc, idx => idx
            .Index(ElasticsearchIndices.Tickets)
            .Id(doc.Id.ToString()), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index ticket {TicketId}: {Reason}", ticket.Id, response.DebugInformation);
        }
    }

    public async Task UpdateTicketAssignmentAsync(
        Guid ticketId,
        Guid? supportAgentId,
        string? supportAgentUserName,
        TicketStatus status,
        DateTime? assignedAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                SupportAgentId = supportAgentId,
                SupportAgentUserName = supportAgentUserName,
                Status = (short)status,
                AssignedAt = assignedAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket assignment {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketStatusAsync(
        Guid ticketId,
        TicketStatus status,
        DateTime? closedAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                Status = (short)status,
                ClosedAt = closedAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket status {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketFeedbackAsync(
        Guid ticketId,
        TicketFeedback feedback,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                Feedback = (short)feedback
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket feedback {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketLastMessageAtAsync(
        Guid ticketId,
        DateTime lastMessageAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                LastMessageAt = lastMessageAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket last message {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task AddUnreadNotificationAsync(
        Guid ticketId,
        TicketNotificationDetailsDto notification,
        CancellationToken cancellationToken)
    {
        var script = """
            if (ctx._source.unreadNotifications == null) {
                ctx._source.unreadNotifications = new ArrayList();
            }
            ctx._source.unreadNotifications.add(params.notification);
            """;

        var response = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Script(s => s
                .Source(script)
                .Params(p => p.Add("notification", new
                {
                    id = notification.Id,
                    organizationId = notification.OrganizationId,
                    ticketId = notification.TicketId,
                    text = notification.Text,
                    status = (short)notification.Status,
                    createdAt = notification.CreatedAt
                }))), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to add unread notification to ticket {TicketId}: {Reason}", ticketId, response.DebugInformation);
        }
    }

    public async Task RemoveUnreadNotificationAsync(
        Guid ticketId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var script = """
            if (ctx._source.unreadNotifications != null) {
                ctx._source.unreadNotifications.removeIf(n -> n.id == params.notificationId);
            }
            """;

        var response = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Script(s => s
                .Source(script)
                .Params(p => p.Add("notificationId", notificationId.ToString()))), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to remove unread notification {NotificationId} from ticket {TicketId}: {Reason}", notificationId, ticketId, response.DebugInformation);
        }
    }

    #endregion
}
