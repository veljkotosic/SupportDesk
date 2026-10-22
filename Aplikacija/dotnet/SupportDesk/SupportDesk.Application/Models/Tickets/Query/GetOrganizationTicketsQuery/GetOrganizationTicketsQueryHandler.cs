using System.Text;
using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Enums;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

internal sealed class GetOrganizationTicketsQueryHandler
    : AbstractQueryHandler<GetOrganizationTicketsQuery, GetOrganizationTicketsQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetOrganizationTicketsQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IDbConnectionFactory dbConnectionFactory)
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationTicketsQueryResult> ExecuteAsync(
        GetOrganizationTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        var dynamicParams = new DynamicParameters();
        dynamicParams.Add("OrganizationId", organizationId);
        dynamicParams.Add("OpenStatus", (short)TicketStatus.Open);
        dynamicParams.Add("AssignedStatus", (short)TicketStatus.Assigned);
        dynamicParams.Add("ClosedStatus", (short)TicketStatus.Closed);
        dynamicParams.Add("Skip", query.Skip);
        dynamicParams.Add("Take", query.Take);

        var whereClause = new StringBuilder("WHERE t.\"OrganizationId\" = @OrganizationId");

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
                whereClause.Append(" AND (t.\"Subject\" ILIKE @SearchPattern OR cu.\"UserName\" ILIKE @SearchPattern OR c.\"Name\" ILIKE @SearchPattern)");
            }
        }

        if (query.Status.HasValue)
        {
            dynamicParams.Add("Status", (short)query.Status.Value);
            whereClause.Append(" AND t.\"Status\" = @Status");
        }

        if (query.Priority.HasValue)
        {
            dynamicParams.Add("Priority", (short)query.Priority.Value);
            whereClause.Append(" AND t.\"Priority\" = @Priority");
        }

        var orderByClause = query.SortBy switch
        {
            TicketSortOption.Oldest => "ORDER BY t.\"LastMessageAt\" ASC NULLS LAST, t.\"Id\" ASC",
            TicketSortOption.Priority => "ORDER BY t.\"Priority\" DESC, t.\"LastMessageAt\" DESC NULLS LAST, t.\"Id\" DESC",
            TicketSortOption.Latest or _ => "ORDER BY t.\"LastMessageAt\" DESC NULLS LAST, t.\"Id\" DESC"
        };

        var hasSearch = !string.IsNullOrWhiteSpace(query.SearchTerm);
        var countFromClause = hasSearch
            ? "FROM \"Tickets\" t JOIN \"Categories\" c ON t.\"CategoryId\" = c.\"Id\" JOIN \"Users\" cu ON t.\"CustomerId\" = cu.\"Id\""
            : "FROM \"Tickets\" t";

        var dataQuery = hasSearch
            ? $"""
                SELECT 
                    t."Id" AS "Id",
                    t."CategoryId" AS "CategoryId",
                    c."Name" AS "CategoryName",
                    t."CustomerId" AS "CustomerId",
                    cu."UserName" AS "CustomerUserName",
                    cu."Email" AS "CustomerEmail",
                    t."SupportAgentId" AS "SupportAgentId",
                    sa."UserName" AS "SupportAgentUserName",
                    t."Status" AS "Status",
                    t."Priority" AS "Priority",
                    t."Feedback" AS "Feedback",
                    t."Subject" AS "Subject",
                    t."LastMessageAt" AS "LastMessageAt"
                FROM "Tickets" t
                JOIN "Categories" c ON t."CategoryId" = c."Id"
                JOIN "Users" cu ON t."CustomerId" = cu."Id"
                LEFT JOIN "Users" sa ON t."SupportAgentId" = sa."Id"
                {whereClause}
                {orderByClause}
                LIMIT @Take OFFSET @Skip;
                """
            : $"""
                WITH paged AS (
                    SELECT t."Id"
                    FROM "Tickets" t
                    {whereClause}
                    {orderByClause}
                    LIMIT @Take OFFSET @Skip
                )
                SELECT 
                    t."Id" AS "Id",
                    t."CategoryId" AS "CategoryId",
                    c."Name" AS "CategoryName",
                    t."CustomerId" AS "CustomerId",
                    cu."UserName" AS "CustomerUserName",
                    cu."Email" AS "CustomerEmail",
                    t."SupportAgentId" AS "SupportAgentId",
                    sa."UserName" AS "SupportAgentUserName",
                    t."Status" AS "Status",
                    t."Priority" AS "Priority",
                    t."Feedback" AS "Feedback",
                    t."Subject" AS "Subject",
                    t."LastMessageAt" AS "LastMessageAt"
                FROM paged p
                JOIN "Tickets" t ON t."Id" = p."Id"
                JOIN "Categories" c ON t."CategoryId" = c."Id"
                JOIN "Users" cu ON t."CustomerId" = cu."Id"
                LEFT JOIN "Users" sa ON t."SupportAgentId" = sa."Id"
                {orderByClause};
                """;

        var sql = $"""
            -- 1. Status counts
            SELECT 
                COUNT(*)::int AS "AllCount",
                COUNT(*) FILTER (WHERE "Status" = @OpenStatus)::int AS "OpenCount",
                COUNT(*) FILTER (WHERE "Status" = @AssignedStatus)::int AS "AssignedCount",
                COUNT(*) FILTER (WHERE "Status" = @ClosedStatus)::int AS "ClosedCount"
            FROM "Tickets"
            WHERE "OrganizationId" = @OrganizationId;

            -- 2. Total filtered count
            SELECT COUNT(*)::int
            {countFromClause}
            {whereClause};

            -- 3. Paginated tickets
            {dataQuery}
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            dynamicParams,
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var statusCounts = await multi.ReadSingleAsync<StatusCountsRow>();
        var totalCount = await multi.ReadSingleAsync<int>();
        var rawTickets = (await multi.ReadAsync<RawOrganizationTicketRow>()).ToList();

        var ticketRows = rawTickets.Select(t => new OrganizationTicketListingDto(
            t.Id,
            t.CategoryId,
            t.CategoryName,
            t.CustomerId,
            t.CustomerUserName,
            t.CustomerEmail,
            t.SupportAgentId,
            t.SupportAgentUserName,
            t.Status,
            t.Priority,
            t.Feedback,
            t.Subject,
            t.LastMessageAt
        )).ToList();

        return new GetOrganizationTicketsQueryResult(
            ticketRows,
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

    private sealed record RawOrganizationTicketRow
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerUserName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public Guid? SupportAgentId { get; set; }
        public string? SupportAgentUserName { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketFeedback Feedback { get; set; }
        public string Subject { get; set; } = null!;
        public DateTime? LastMessageAt { get; set; }
    }
}