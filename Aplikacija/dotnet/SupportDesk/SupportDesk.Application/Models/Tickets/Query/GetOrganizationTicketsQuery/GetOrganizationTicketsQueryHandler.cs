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

        var whereClause = new StringBuilder("WHERE organization_id = @OrganizationId");

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
                whereClause.Append(" AND (subject ILIKE @SearchPattern OR customer_user_name ILIKE @SearchPattern OR category_name ILIKE @SearchPattern)");
            }
        }

        if (query.Status.HasValue)
        {
            dynamicParams.Add("Status", (short)query.Status.Value);
            whereClause.Append(" AND status = @Status");
        }

        if (query.Priority.HasValue)
        {
            dynamicParams.Add("Priority", (short)query.Priority.Value);
            whereClause.Append(" AND priority = @Priority");
        }

        var orderByClause = query.SortBy switch
        {
            TicketSortOption.Oldest => "ORDER BY last_message_at ASC NULLS LAST, id ASC",
            TicketSortOption.Priority => "ORDER BY priority DESC, last_message_at DESC NULLS LAST, id DESC",
            TicketSortOption.Latest or _ => "ORDER BY last_message_at DESC NULLS LAST, id DESC"
        };

        var sql = $"""
            -- 1. Status counts
            SELECT 
                COUNT(*)::int AS "AllCount",
                COUNT(*) FILTER (WHERE status = @OpenStatus)::int AS "OpenCount",
                COUNT(*) FILTER (WHERE status = @AssignedStatus)::int AS "AssignedCount",
                COUNT(*) FILTER (WHERE status = @ClosedStatus)::int AS "ClosedCount"
            FROM projections.ticket_summaries
            WHERE organization_id = @OrganizationId;

            -- 2. Total filtered count
            SELECT COUNT(*)::int
            FROM projections.ticket_summaries
            {whereClause};

            -- 3. Paginated tickets
            SELECT 
                id AS "Id",
                category_id AS "CategoryId",
                category_name AS "CategoryName",
                customer_id AS "CustomerId",
                customer_user_name AS "CustomerUserName",
                customer_email AS "CustomerEmail",
                support_agent_id AS "SupportAgentId",
                support_agent_user_name AS "SupportAgentUserName",
                status AS "Status",
                priority AS "Priority",
                feedback AS "Feedback",
                subject AS "Subject",
                last_message_at AS "LastMessageAt"
            FROM projections.ticket_summaries
            {whereClause}
            {orderByClause}
            LIMIT @Take OFFSET @Skip;
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