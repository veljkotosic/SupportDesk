using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Organization.Validation;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationFaqs;

internal sealed class GetOrganizationFaqsQueryHandler
    : AbstractQueryHandler<GetOrganizationFaqsQuery, GetOrganizationFaqsQueryResult>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    
    public GetOrganizationFaqsQueryHandler(
        PermissionChecker permissionChecker,
        IReadDbConnectionFactory readDbConnectionFactory)
        : base(permissionChecker)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    protected override async Task<GetOrganizationFaqsQueryResult> ExecuteAsync(GetOrganizationFaqsQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(query.OrganizationId);

        const string sql = """
            -- 1. Check organization existence
            SELECT EXISTS (
                SELECT 1 FROM "Organizations" WHERE "Id" = @OrganizationId AND "DeletedAt" IS NULL
            );

            -- 2. Fetch FAQs
            SELECT "Id", "Question", "Answer"
            FROM "Faqs"
            WHERE "OrganizationId" = @OrganizationId AND "DeletedAt" IS NULL
            ORDER BY "Question";
            """;

        await using var connection = await _readDbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { OrganizationId = query.OrganizationId }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var exists = await multi.ReadFirstAsync<bool>();
        if (!exists)
        {
            throw new ValidationException(OrganizationErrors.NotFound(organizationId));
        }

        var faqs = (await multi.ReadAsync<FaqListingDto>()).AsList();

        return new GetOrganizationFaqsQueryResult(faqs);       
    }
}