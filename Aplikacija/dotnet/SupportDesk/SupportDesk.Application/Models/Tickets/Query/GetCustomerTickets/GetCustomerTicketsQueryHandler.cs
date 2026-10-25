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
    private readonly IReadDbConnectionFactory _dbConnectionFactory;

    public GetCustomerTicketsQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        IReadDbConnectionFactory dbConnectionFactory)
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

        var whereClause = new StringBuilder("WHERE t.\"CustomerId\" = @CustomerId");

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim();
            if (Guid.TryParse(search, out var parsedGuid))
            {
                dynamicParams.Add("SearchGuid", parsedGuid);
                whereClause.Append(" AND t.\"Id\" = @SearchGuid");
            }
            else
            {
                dynamicParams.Add("SearchPattern", $"%{search}%");
                whereClause.Append(" AND (t.\"Subject\" ILIKE @SearchPattern OR o.\"Name\" ILIKE @SearchPattern OR c.\"Name\" ILIKE @SearchPattern)");
            }
        }

        if (query.Status.HasValue)
        {
            dynamicParams.Add("Status", (short)query.Status.Value);
            whereClause.Append(" AND t.\"Status\" = @Status");
        }

        var hasSearch = !string.IsNullOrWhiteSpace(query.SearchTerm);
        var countFromClause = hasSearch
            ? "FROM \"Tickets\" t JOIN \"Organizations\" o ON t.\"OrganizationId\" = o.\"Id\" JOIN \"Categories\" c ON t.\"CategoryId\" = c.\"Id\" JOIN \"Users\" cu ON t.\"CustomerId\" = cu.\"Id\""
            : "FROM \"Tickets\" t";

        var paginatedPageFromClause = hasSearch
            ? "FROM \"Tickets\" t JOIN \"Organizations\" o ON t.\"OrganizationId\" = o.\"Id\" JOIN \"Categories\" c ON t.\"CategoryId\" = c.\"Id\" JOIN \"Users\" cu ON t.\"CustomerId\" = cu.\"Id\""
            : "FROM \"Tickets\" t";

        var sql = $"""
            -- 1. Status counts
            SELECT 
                COUNT(*)::int AS "AllCount",
                COUNT(*) FILTER (WHERE "Status" = @OpenStatus)::int AS "OpenCount",
                COUNT(*) FILTER (WHERE "Status" = @AssignedStatus)::int AS "AssignedCount",
                COUNT(*) FILTER (WHERE "Status" = @ClosedStatus)::int AS "ClosedCount"
            FROM "Tickets"
            WHERE "CustomerId" = @CustomerId;

            -- 2. Total filtered count
            SELECT COUNT(*)::int
            {countFromClause}
            {whereClause};

            -- 3. Paginated tickets
            SELECT 
                t."Id" AS "Id",
                t."OrganizationId" AS "OrganizationId",
                o."Name" AS "OrganizationName",
                t."CategoryId" AS "CategoryId",
                c."Name" AS "CategoryName",
                t."CustomerId" AS "CustomerId",
                cu."UserName" AS "CustomerUserName",
                t."SupportAgentId" AS "SupportAgentId",
                sa."UserName" AS "SupportAgentUserName",
                t."Status" AS "Status",
                t."Priority" AS "Priority",
                t."Feedback" AS "Feedback",
                t."Subject" AS "Subject",
                t."OpenedAt" AS "OpenedAt",
                t."AssignedAt" AS "AssignedAt",
                t."ClosedAt" AS "ClosedAt",
                t."LastMessageAt" AS "LastMessageAt"
            FROM "Tickets" t
            JOIN "Organizations" o ON t."OrganizationId" = o."Id"
            JOIN "Categories" c ON t."CategoryId" = c."Id"
            JOIN "Users" cu ON t."CustomerId" = cu."Id"
            LEFT JOIN "Users" sa ON t."SupportAgentId" = sa."Id"
            {whereClause}
            ORDER BY t."LastMessageAt" DESC NULLS LAST, t."Id" DESC
            LIMIT @Take OFFSET @Skip;

            -- 4. Unread notifications for paginated tickets
            WITH paginated_page AS (
                SELECT t."Id"
                {paginatedPageFromClause}
                {whereClause}
                ORDER BY t."LastMessageAt" DESC NULLS LAST, t."Id" DESC
                LIMIT @Take OFFSET @Skip
            )
            SELECT 
                n."Id" AS "Id",
                n."OrganizationId" AS "OrganizationId",
                n."TicketId" AS "TicketId",
                n."Text" AS "Text",
                n."Status" AS "Status",
                n."CreatedAt" AS "CreatedAt"
            FROM "TicketNotifications" n
            JOIN paginated_page p ON n."TicketId" = p."Id"
            WHERE n."Status" = @UnreadNotificationStatus;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            dynamicParams,
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var statusCounts = await multi.ReadSingleAsync<StatusCountsRow>();
        var totalCount = await multi.ReadSingleAsync<int>();
        var rawTickets = (await multi.ReadAsync<RawCustomerTicketRow>()).ToList();
        var rawNotifications = (await multi.ReadAsync<RawNotificationRow>()).ToList();

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