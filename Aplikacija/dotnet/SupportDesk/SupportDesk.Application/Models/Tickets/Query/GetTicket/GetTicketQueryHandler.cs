using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicket;

internal sealed class GetTicketQueryHandler
    : AbstractQueryHandler<GetTicketQuery, GetTicketQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;

    public GetTicketQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITenantContext tenantContext,
        IReadDbConnectionFactory readDbConnectionFactory)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    protected override async Task<GetTicketQueryResult> ExecuteAsync(
        GetTicketQuery query,
        CancellationToken cancellationToken)
    {
        var ticketId = query.TicketId;
        var currentUserId = _userContext.GetCurrentUserId();
        var organizationId = _tenantContext.GetCurrentOrganizationId();

        const string sql = """
            -- 1. Ticket details
            SELECT 
                id AS "Id",
                organization_id AS "OrganizationId",
                organization_name AS "OrganizationName",
                category_id AS "CategoryId",
                category_name AS "CategoryName",
                customer_id AS "CustomerId",
                customer_user_name AS "CustomerUserName",
                support_agent_id AS "SupportAgentId",
                support_agent_user_name AS "SupportAgentUserName",
                status AS "Status",
                priority AS "Priority",
                feedback AS "Feedback",
                subject AS "Subject",
                opened_at AS "OpenedAt",
                assigned_at AS "AssignedAt",
                closed_at AS "ClosedAt",
                last_message_at AS "LastMessageAt"
            FROM projections.ticket_summaries
            WHERE id = @TicketId;

            -- 2. Unread notifications
            SELECT 
                n."Id" AS "Id",
                n."OrganizationId" AS "OrganizationId",
                n."TicketId" AS "TicketId",
                n."Text" AS "Text",
                n."Status" AS "Status",
                n."CreatedAt" AS "CreatedAt"
            FROM "TicketNotifications" n
            WHERE n."TicketId" = @TicketId
              AND n."Status" = @UnreadStatus;
            """;

        await using var connection = await _readDbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { TicketId = ticketId, UnreadStatus = (short)TicketNotificationStatus.Unread },
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var rawTicket = await multi.ReadFirstOrDefaultAsync<RawTicketRow>();

        if (rawTicket is null || (organizationId is null && rawTicket.CustomerId != currentUserId))
        {
            throw new ValidationException(TicketErrors.NotFound(new TicketId(ticketId)));
        }

        var rawNotifications = (await multi.ReadAsync<RawNotificationRow>()).ToList();
        var notifications = rawNotifications.Select(n => new TicketNotificationDetailsDto(
            n.Id,
            n.OrganizationId,
            n.TicketId,
            n.Text,
            n.Status,
            n.CreatedAt)).ToList();

        var details = new DashboardTicketDetailsDto(
            rawTicket.Id,
            rawTicket.OrganizationId,
            rawTicket.OrganizationName,
            rawTicket.CategoryId,
            rawTicket.CategoryName,
            rawTicket.CustomerId,
            rawTicket.CustomerUserName,
            rawTicket.SupportAgentId,
            rawTicket.SupportAgentUserName,
            rawTicket.Status,
            rawTicket.Priority,
            rawTicket.Subject,
            rawTicket.OpenedAt,
            rawTicket.AssignedAt,
            rawTicket.ClosedAt,
            rawTicket.Feedback,
            rawTicket.LastMessageAt,
            notifications);

        return new GetTicketQueryResult(details);
    }

    private sealed record RawTicketRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerUserName { get; set; } = null!;
        public Guid? SupportAgentId { get; set; }
        public string? SupportAgentUserName { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketFeedback Feedback { get; set; }
        public string Subject { get; set; } = null!;
        public DateTime OpenedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    private sealed record RawNotificationRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; } = null!;
        public TicketNotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}