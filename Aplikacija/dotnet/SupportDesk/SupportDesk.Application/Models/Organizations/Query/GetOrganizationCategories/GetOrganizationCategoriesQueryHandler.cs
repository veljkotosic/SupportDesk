using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Organization.Validation;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationCategories;

internal sealed class GetOrganizationCategoriesQueryHandler
    : AbstractQueryHandler<GetOrganizationCategoriesQuery, GetOrganizationCategoriesQueryResult>
{
    private readonly IReadDbConnectionFactory _dbConnectionFactory;
    
    public GetOrganizationCategoriesQueryHandler(
        PermissionChecker permissionChecker,
        IReadDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetOrganizationCategoriesQueryResult> ExecuteAsync(GetOrganizationCategoriesQuery query, CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(query.OrganizationId);

        const string sql = """
            -- 1. Check organization existence
            SELECT EXISTS (
                SELECT 1 FROM "Organizations" WHERE "Id" = @OrganizationId AND "DeletedAt" IS NULL
            );

            -- 2. Fetch categories
            SELECT "Id" AS "CategoryId", "Name" AS "CategoryName"
            FROM "Categories"
            WHERE "OrganizationId" = @OrganizationId AND "DeletedAt" IS NULL
            ORDER BY "Name";
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { OrganizationId = query.OrganizationId }, cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);

        var exists = await multi.ReadFirstAsync<bool>();
        if (!exists)
        {
            throw new ValidationException(OrganizationErrors.NotFound(organizationId));
        }

        var categories = (await multi.ReadAsync<CategoryListingDto>()).AsList();

        return new GetOrganizationCategoriesQueryResult(categories);
    }
}