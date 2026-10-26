using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

internal sealed class GetOrganizationAdminDashboardQueryHandler
    : AbstractQueryHandler<GetOrganizationAdminDashboardQuery, GetOrganizationAdminDashboardQueryResult>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetOrganizationAdminDashboardQueryHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        IDbConnectionFactory dbConnectionFactory)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationAdminDashboardQueryResult> ExecuteAsync(GetOrganizationAdminDashboardQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId((Guid)_tenantContext.GetCurrentOrganizationId()!);

        var startDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-6);

        const string sql = """
            -- 1. Organization name
            SELECT organization_name AS "Name"
            FROM projections.organization_dashboards
            WHERE organization_id = @OrganizationId;
            
            -- 2. Dashboard summary
            SELECT
                open_tickets AS "OpenTickets",
                assigned_tickets AS "AssignedTickets",
                closed_tickets AS "ClosedTickets",
                agent_count AS "AgentCount"
            FROM projections.organization_dashboards
            WHERE organization_id = @OrganizationId;
            
            -- 3. Support agents and assigned-ticket counts
            SELECT
                id AS "Id",
                user_name AS "UserName",
                assigned_tickets AS "AssignedTicketCount"
            FROM projections.support_agents
            WHERE organization_id = @OrganizationId
            ORDER BY user_name;
            
            -- 4. Six most recent tickets
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
                subject AS "Subject",
                opened_at AS "OpenedAt",
                assigned_at AS "AssignedAt",
                closed_at AS "ClosedAt",
                feedback AS "Feedback",
                last_message_at AS "LastMessageAt"
            FROM projections.ticket_summaries
            WHERE organization_id = @OrganizationId
            ORDER BY last_message_at DESC NULLS LAST, id DESC
            LIMIT 6;
            
            -- 5. Unread notifications for the six most recent tickets
            WITH recent_tickets AS (
                SELECT id
                FROM projections.ticket_summaries
                WHERE organization_id = @OrganizationId
                ORDER BY last_message_at DESC NULLS LAST, id DESC
                LIMIT 6
            )
            SELECT
                n."Id" AS "Id",
                n."OrganizationId" AS "OrganizationId",
                n."TicketId" AS "TicketId",
                n."Text" AS "Text",
                n."Status" AS "Status",
                n."CreatedAt" AS "CreatedAt"
            FROM public."TicketNotifications" n
            JOIN recent_tickets rt ON rt.id = n."TicketId"
            WHERE n."OrganizationId" = @OrganizationId
              AND n."Status" = @UnreadStatus;
            
            -- 6. Ticket volume per day
            WITH date_series AS (
                SELECT generate_series(@StartDate::date, @StartDate::date + 6, INTERVAL '1 day')::date AS "Day"
            )
            SELECT
                ds."Day" AS "Date",
                COALESCE(dv.opened_count, 0) AS "OpenedCount",
                COALESCE(dv.closed_count, 0) AS "ClosedCount"
            FROM date_series ds
            LEFT JOIN projections.daily_ticket_volumes dv 
                ON dv.organization_id = @OrganizationId AND dv.day = ds."Day"
            ORDER BY ds."Day";
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new
        {
            OrganizationId = organizationId.IdValue,
            UnreadStatus = (short)TicketNotificationStatus.Unread,
            StartDate = startDate
        };

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        await using var results = await connection.QueryMultipleAsync(command);

        var organization = await results.ReadSingleAsync<OrganizationRow>();

        var summaryRow = await results.ReadSingleAsync<SummaryRow>();

        var summary = new OrganizationAdminDashboardSummaryDto(
            summaryRow.OpenTickets,
            summaryRow.AssignedTickets,
            summaryRow.ClosedTickets,
            summaryRow.AgentCount);

        var agentRows = (await results.ReadAsync<AgentRow>()).ToList();

        var agents = agentRows
            .Select(agent => new OrganizationAdminDashboardAgentDto(
                agent.Id,
                agent.UserName,
                agent.AssignedTicketCount))
            .ToList();

        var ticketRows = (await results.ReadAsync<TicketRow>()).ToList();

        var notificationRows = (await results.ReadAsync<NotificationRow>()).ToList();

        var notificationsByTicket = notificationRows
            .GroupBy(notification => notification.TicketId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(notification =>
                    new TicketNotificationDetailsDto(
                        notification.Id,
                        notification.OrganizationId,
                        notification.TicketId,
                        notification.Text,
                        notification.Status,
                        notification.CreatedAt))
                    .ToList());

        var recentTickets = ticketRows
            .Select(ticket => new DashboardTicketDetailsDto(
                ticket.Id,
                ticket.OrganizationId,
                ticket.OrganizationName,
                ticket.CategoryId,
                ticket.CategoryName,
                ticket.CustomerId,
                ticket.CustomerUserName,
                ticket.SupportAgentId,
                ticket.SupportAgentUserName,
                ticket.Status,
                ticket.Priority,
                ticket.Subject,
                ticket.OpenedAt,
                ticket.AssignedAt,
                ticket.ClosedAt,
                ticket.Feedback,
                ticket.LastMessageAt,
                notificationsByTicket.GetValueOrDefault(ticket.Id) ?? []))
            .ToList();

        var volumeRows = (await results.ReadAsync<VolumeRow>()).ToList();

        var ticketVolume = volumeRows
            .Select(row => new OrganizationAdminDashboardTicketVolumeEntryDto(
                row.Date,
                row.OpenedCount,
                row.ClosedCount))
            .ToList();

        return new GetOrganizationAdminDashboardQueryResult(
            organization.Name,
            summary,
            ticketVolume,
            agents,
            recentTickets);
    }

    private sealed record OrganizationRow
    {
        public string Name { get; set; } = null!;
    }

    private sealed record SummaryRow
    {
        public int OpenTickets { get; set; }
        public int AssignedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int AgentCount { get; set; }
    }

    private sealed record AgentRow
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public int AssignedTicketCount { get; set; }
    }

    private sealed record TicketRow
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
        public string Subject { get; set; } = null!;
        public DateTime OpenedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public TicketFeedback Feedback { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    private sealed record NotificationRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; } = null!;
        public TicketNotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed record VolumeRow
    {
        public DateOnly Date { get; set; }
        public int OpenedCount { get; set; }
        public int ClosedCount { get; set; }
    }
}
