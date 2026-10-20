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
using SupportDesk.Domain.Models.User.Enums;

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
            SELECT o."Name"
            FROM "Organizations" o
            WHERE o."Id" = @OrganizationId;
            
            -- 2. Dashboard summary
            SELECT
                COUNT(*) FILTER (WHERE t."Status" = @OpenStatus) AS "OpenTickets",
                COUNT(*) FILTER (WHERE t."Status" = @AssignedStatus) AS "AssignedTickets",
                COUNT(*) FILTER (WHERE t."Status" = @ClosedStatus) AS "ClosedTickets",
                (
                    SELECT COUNT(*)
                    FROM "Users" u
                    WHERE u."OrganizationId" = @OrganizationId
                      AND u."Role" = @SupportAgentRole
                ) AS "AgentCount"
            FROM "Tickets" t
            WHERE t."OrganizationId" = @OrganizationId;
            
            -- 3. Support agents and assigned-ticket counts
            SELECT
                u."Id" AS "Id",
                u."UserName" AS "UserName",
                COUNT(t."Id") AS "AssignedTicketCount"
            FROM "Users" u
            LEFT JOIN "Tickets" t
                ON t."SupportAgentId" = u."Id"
               AND t."OrganizationId" = @OrganizationId
               AND t."Status" = @AssignedStatus
            WHERE u."OrganizationId" = @OrganizationId
              AND u."Role" = @SupportAgentRole
            GROUP BY u."Id", u."UserName"
            ORDER BY u."UserName";
            
            -- 4. Six most recent tickets
            SELECT
                t."Id" AS "Id",
                o."Id" AS "OrganizationId",
                o."Name" AS "OrganizationName",
                c."Id" AS "CategoryId",
                c."Name" AS "CategoryName",
                customer."Id" AS "CustomerId",
                customer."UserName" AS "CustomerUserName",
                agent."Id" AS "SupportAgentId",
                agent."UserName" AS "SupportAgentUserName",
                t."Status" AS "Status",
                t."Priority" AS "Priority",
                t."Subject" AS "Subject",
                t."OpenedAt" AS "OpenedAt",
                t."AssignedAt" AS "AssignedAt",
                t."ClosedAt" AS "ClosedAt",
                t."Feedback" AS "Feedback",
                t."LastMessageAt" AS "LastMessageAt"
            FROM "Tickets" t
            JOIN "Organizations" o
                ON o."Id" = t."OrganizationId"
            JOIN "Categories" c
                ON c."Id" = t."CategoryId"
            JOIN "Users" customer
                ON customer."Id" = t."CustomerId"
            LEFT JOIN "Users" agent
                ON agent."Id" = t."SupportAgentId"
            WHERE t."OrganizationId" = @OrganizationId
            ORDER BY t."LastMessageAt" DESC NULLS LAST
            LIMIT 6;
            
            -- 5. Unread notifications for the six most recent tickets
            WITH recent_tickets AS (
                SELECT t."Id"
                FROM "Tickets" t
                WHERE t."OrganizationId" = @OrganizationId
                ORDER BY t."LastMessageAt" DESC NULLS LAST
                LIMIT 6
            )
            SELECT
                n."Id" AS "Id",
                n."OrganizationId" AS "OrganizationId",
                n."TicketId" AS "TicketId",
                n."Text" AS "Text",
                n."Status" AS "Status",
                n."CreatedAt" AS "CreatedAt"
            FROM "TicketNotifications" n
            JOIN recent_tickets rt
                ON rt."Id" = n."TicketId"
            WHERE n."OrganizationId" = @OrganizationId
              AND n."Status" = @UnreadStatus;
            
            -- 6. Ticket volume per day
            WITH date_series AS (
                SELECT generate_series(@StartDate::date, @StartDate::date + 6, INTERVAL '1 day')::date AS "Day"
            ),
            opened_counts AS (
                SELECT "OpenedAt"::date AS "Day", COUNT(*)::int AS "Count"
                FROM "Tickets"
                WHERE "OrganizationId" = @OrganizationId
                  AND "OpenedAt" >= @StartDate::date
                  AND "OpenedAt" < (@StartDate::date + 7)
                GROUP BY "OpenedAt"::date
            ),
            closed_counts AS (
                SELECT "ClosedAt"::date AS "Day", COUNT(*)::int AS "Count"
                FROM "Tickets"
                WHERE "OrganizationId" = @OrganizationId
                  AND "Status" = @ClosedStatus
                  AND "ClosedAt" >= @StartDate::date
                  AND "ClosedAt" < (@StartDate::date + 7)
                GROUP BY "ClosedAt"::date
            )
            SELECT
                ds."Day" AS "Date",
                COALESCE(oc."Count", 0) AS "OpenedCount",
                COALESCE(cc."Count", 0) AS "ClosedCount"
            FROM date_series ds
            LEFT JOIN opened_counts oc ON ds."Day" = oc."Day"
            LEFT JOIN closed_counts cc ON ds."Day" = cc."Day"
            ORDER BY ds."Day";
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var parameters = new
        {
            OrganizationId = organizationId.IdValue,
            OpenStatus = TicketStatus.Open,
            AssignedStatus = TicketStatus.Assigned,
            ClosedStatus = TicketStatus.Closed,
            SupportAgentRole = UserRole.SupportAgent,
            UnreadStatus = TicketNotificationStatus.Unread,
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
