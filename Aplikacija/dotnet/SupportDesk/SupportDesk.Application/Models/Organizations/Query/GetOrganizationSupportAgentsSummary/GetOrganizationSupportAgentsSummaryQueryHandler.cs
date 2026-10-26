using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;

internal sealed class GetOrganizationSupportAgentsSummaryQueryHandler
    : AbstractQueryHandler<GetOrganizationSupportAgentsSummaryQuery, GetOrganizationSupportAgentsSummaryQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public GetOrganizationSupportAgentsSummaryQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IDbConnectionFactory dbConnectionFactory) 
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
                id AS "Id",
                user_name AS "UserName",
                email AS "Email",
                assigned_tickets AS "OpenTickets",
                resolved_tickets AS "ResolvedTickets",
                joined_at AS "JoinedAt"
            FROM projections.support_agents
            WHERE organization_id = @OrganizationId
            ORDER BY user_name;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken);

        var agents = (await connection.QueryAsync<SupportAgentSummary>(command)).AsList();

        return new GetOrganizationSupportAgentsSummaryQueryResult(agents);
    }
}