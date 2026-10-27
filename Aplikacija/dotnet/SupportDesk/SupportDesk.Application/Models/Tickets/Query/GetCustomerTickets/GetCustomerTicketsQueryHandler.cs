using System.Text;
using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

internal sealed class GetCustomerTicketsQueryHandler
    : AbstractQueryHandler<GetCustomerTicketsQuery, GetCustomerTicketsQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetCustomerTicketsQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        IDbConnectionFactory dbConnectionFactory)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetCustomerTicketsQueryResult> ExecuteAsync(
        GetCustomerTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = _userContext.GetCurrentUserId();

        var dynamicParams = new DynamicParameters();
        dynamicParams.Add("CustomerId", customerId);
        dynamicParams.Add("OpenStatus", (short)TicketStatus.Open);
        dynamicParams.Add("AssignedStatus", (short)TicketStatus.Assigned);
        dynamicParams.Add("ClosedStatus", (short)TicketStatus.Closed);
        dynamicParams.Add("UnreadNotificationStatus", (short)TicketNotificationStatus.Unread);
        dynamicParams.Add("Skip", query.Skip);
        dynamicParams.Add("Take", query.Take);

        var whereClause = new StringBuilder("WHERE customer_id = @CustomerId");

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim();
            if (Guid.TryParse(search, out var parsedGuid))
            {
                dynamicParams.Add("SearchGuid", parsedGuid);
                whereClause.Append(" AND id = @SearchGuid");
            }
            else
            {
                dynamicParams.Add("SearchPattern", $"%{search}%");
                whereClause.Append(" AND (subject ILIKE @SearchPattern OR organization_name ILIKE @SearchPattern OR category_name ILIKE @SearchPattern)");
            }
        }

        if (query.Status.HasValue)
        {
            dynamicParams.Add("Status", (short)query.Status.Value);
            whereClause.Append(" AND status = @Status");
        }

        var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);

        var sqlBuilder = new StringBuilder();

        // 1. Status counts
        sqlBuilder.AppendLine("""
            SELECT 
                COUNT(*)::int AS "AllCount",
                COUNT(*) FILTER (WHERE status = @OpenStatus)::int AS "OpenCount",
                COUNT(*) FILTER (WHERE status = @AssignedStatus)::int AS "AssignedCount",
                COUNT(*) FILTER (WHERE status = @ClosedStatus)::int AS "ClosedCount"
            FROM projections.ticket_summaries
            WHERE customer_id = @CustomerId;
            """);

        // 2. Total filtered count (only needed when searching)
        if (hasSearchTerm)
        {
            sqlBuilder.AppendLine($"""
                SELECT COUNT(*)::int
                FROM projections.ticket_summaries
                {whereClause};
                """);
        }

        // 3. Paginated tickets
        sqlBuilder.AppendLine($"""
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
                last_message_at AS "LastMessageAt",
                unread_notifications_count AS "UnreadNotificationsCount"
            FROM projections.ticket_summaries
            {whereClause}
            ORDER BY last_message_at DESC NULLS LAST, id DESC
            LIMIT @Take OFFSET @Skip;
            """);

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sqlBuilder.ToString(),
            dynamicParams,
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var statusCounts = await multi.ReadSingleAsync<StatusCountsRow>();

        int totalCount;
        if (hasSearchTerm)
        {
            totalCount = await multi.ReadSingleAsync<int>();
        }
        else if (query.Status.HasValue)
        {
            totalCount = query.Status.Value switch
            {
                TicketStatus.Open => statusCounts.OpenCount,
                TicketStatus.Assigned => statusCounts.AssignedCount,
                TicketStatus.Closed => statusCounts.ClosedCount,
                _ => statusCounts.AllCount
            };
        }
        else
        {
            totalCount = statusCounts.AllCount;
        }

        var rawTickets = (await multi.ReadAsync<RawCustomerTicketRow>()).ToList();

        var ticketIdsWithUnread = rawTickets
            .Where(t => t.UnreadNotificationsCount > 0)
            .Select(t => t.Id)
            .ToArray();

        List<RawNotificationRow> rawNotifications = [];
        if (ticketIdsWithUnread.Length > 0)
        {
            const string notifSql = """
                SELECT 
                    n."Id" AS "Id",
                    n."OrganizationId" AS "OrganizationId",
                    n."TicketId" AS "TicketId",
                    n."Text" AS "Text",
                    n."Status" AS "Status",
                    n."CreatedAt" AS "CreatedAt"
                FROM public."TicketNotifications" n
                WHERE n."TicketId" = ANY(@TicketIds)
                  AND n."Status" = @UnreadNotificationStatus;
                """;

            var notifCommand = new CommandDefinition(
                notifSql,
                new { TicketIds = ticketIdsWithUnread, UnreadNotificationStatus = (short)TicketNotificationStatus.Unread },
                cancellationToken: cancellationToken);

            rawNotifications = (await connection.QueryAsync<RawNotificationRow>(notifCommand)).ToList();
        }

        var notificationsByTicket = rawNotifications
            .GroupBy(n => n.TicketId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<TicketNotificationDetailsDto>)g.Select(n => new TicketNotificationDetailsDto(
                    n.Id,
                    n.OrganizationId,
                    n.TicketId,
                    n.Text,
                    n.Status,
                    n.CreatedAt)).ToList());

        var tickets = rawTickets.Select(t => new CustomerTicketListingDto(
            t.Id,
            t.OrganizationId,
            t.OrganizationName,
            t.CategoryId,
            t.CategoryName,
            t.CustomerId,
            t.CustomerUserName,
            t.SupportAgentId,
            t.SupportAgentUserName,
            t.Status,
            t.Priority,
            t.Feedback,
            t.Subject,
            t.OpenedAt,
            t.AssignedAt,
            t.ClosedAt,
            t.LastMessageAt,
            notificationsByTicket.GetValueOrDefault(t.Id) ?? []
        )).ToList();

        return new GetCustomerTicketsQueryResult(
            tickets,
            totalCount,
            statusCounts.AllCount,
            statusCounts.OpenCount,
            statusCounts.AssignedCount,
            statusCounts.ClosedCount);
    }

    private sealed record StatusCountsRow
    {
        public int AllCount { get; set; }
        public int OpenCount { get; set; }
        public int AssignedCount { get; set; }
        public int ClosedCount { get; set; }
    }

    private sealed record RawCustomerTicketRow
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
        public int UnreadNotificationsCount { get; set; }
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