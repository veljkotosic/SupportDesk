using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase;

internal sealed class GetOrganizationKnowledgeBaseQueryHandler
    : AbstractQueryHandler<GetOrganizationKnowledgeBaseQuery, GetOrganizationKnowledgeBaseQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly IReadDbConnectionFactory _dbConnectionFactory;
    
    public GetOrganizationKnowledgeBaseQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        IReadDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationKnowledgeBaseQueryResult> ExecuteAsync(GetOrganizationKnowledgeBaseQuery query, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        const string sql = """
            -- 1. FAQs
            SELECT "Id", "Question", "Answer"
            FROM "Faqs"
            WHERE "OrganizationId" = @OrganizationId AND "DeletedAt" IS NULL
            ORDER BY "Question";

            -- 2. Template Answers
            SELECT "Id", "Title", "Text"
            FROM "TemplateAnswers"
            WHERE "OrganizationId" = @OrganizationId AND "DeletedAt" IS NULL
            ORDER BY "Title";

            -- 3. Categories with ticket count
            SELECT 
                c."Id",
                c."Name",
                c."Description",
                COALESCE(tc."TicketCount", 0)::int AS "TicketCount"
            FROM "Categories" c
            LEFT JOIN (
                SELECT "CategoryId", COUNT(*)::int AS "TicketCount"
                FROM "Tickets"
                WHERE "OrganizationId" = @OrganizationId
                GROUP BY "CategoryId"
            ) tc ON c."Id" = tc."CategoryId"
            WHERE c."OrganizationId" = @OrganizationId AND c."DeletedAt" IS NULL
            ORDER BY c."Name";
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var faqs = (await multi.ReadAsync<KnowledgeBaseFaqDto>()).AsList();
        var templateAnswers = (await multi.ReadAsync<KnowledgeBaseTemplateAnswerDto>()).AsList();
        var categories = (await multi.ReadAsync<KnowledgeBaseCategoryDto>()).AsList();

        return new GetOrganizationKnowledgeBaseQueryResult(
            organizationId,
            faqs,
            templateAnswers,
            categories);
    }
}