using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;

internal sealed class GetOrganizationSupportAgentsSummaryQueryHandler
    : AbstractQueryHandler<GetOrganizationSupportAgentsSummaryQuery, GetOrganizationSupportAgentsSummaryQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IReadDbConnectionFactory _dbConnectionFactory;
    
    public GetOrganizationSupportAgentsSummaryQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IReadDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationSupportAgentsSummaryQueryResult> ExecuteAsync(GetOrganizationSupportAgentsSummaryQuery query, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        const string sql = """
            SELECT 
                u."Id",
                u."UserName",
                u."Email",
                COALESCE(tc."AssignedCount", 0)::int AS "OpenTickets",
                COALESCE(tc."ClosedCount", 0)::int AS "ResolvedTickets",
                u."CreatedAt" AS "JoinedAt"
            FROM "Users" u
            LEFT JOIN (
                SELECT 
                    "SupportAgentId",
                    COUNT(*) FILTER (WHERE "Status" = @AssignedStatus)::int AS "AssignedCount",
                    COUNT(*) FILTER (WHERE "Status" = @ClosedStatus)::int AS "ClosedCount"
                FROM "Tickets"
                WHERE "OrganizationId" = @OrganizationId AND "SupportAgentId" IS NOT NULL
                GROUP BY "SupportAgentId"
            ) tc ON u."Id" = tc."SupportAgentId"
            WHERE u."OrganizationId" = @OrganizationId AND u."Role" = @SupportAgentRole
            ORDER BY u."UserName";
            """;

        var parameters = new
        {
            OrganizationId = organizationId,
            AssignedStatus = (short)TicketStatus.Assigned,
            ClosedStatus = (short)TicketStatus.Closed,
            SupportAgentRole = (short)UserRole.SupportAgent
        };

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var agents = (await connection.QueryAsync<SupportAgentSummary>(command)).AsList();

        return new GetOrganizationSupportAgentsSummaryQueryResult(agents);
    }
}