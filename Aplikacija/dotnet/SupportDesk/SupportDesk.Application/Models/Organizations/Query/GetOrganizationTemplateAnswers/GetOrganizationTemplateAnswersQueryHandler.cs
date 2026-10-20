using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers;

internal sealed class GetOrganizationTemplateAnswersQueryHandler
    : AbstractQueryHandler<GetOrganizationTemplateAnswersQuery, GetOrganizationTemplateAnswersQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public GetOrganizationTemplateAnswersQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationTemplateAnswersQueryResult> ExecuteAsync(GetOrganizationTemplateAnswersQuery query, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        const string sql = """
            SELECT "Id", "Title", "Text"
            FROM "TemplateAnswers"
            WHERE "OrganizationId" = @OrganizationId AND "DeletedAt" IS NULL
            ORDER BY "Title";
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken);

        var templateAnswers = (await connection.QueryAsync<TemplateAnswerListingDto>(command)).AsList();

        return new GetOrganizationTemplateAnswersQueryResult(templateAnswers);      
    }
}