using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations.Dtos;

namespace SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations;

internal sealed class GetAllOrganizationsQueryHandler
    : AbstractQueryHandler<GetAllOrganizationsQuery, GetAllOrganizationsQueryResult>
{
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    
    public GetAllOrganizationsQueryHandler(
        PermissionChecker permissionChecker,
        IReadDbConnectionFactory readDbConnectionFactory) 
        : base(permissionChecker)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    protected override async Task<GetAllOrganizationsQueryResult> ExecuteAsync(GetAllOrganizationsQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT "Id" AS "OrganizationId", "Name" AS "OrganizationName"
            FROM "Organizations"
            WHERE "DeletedAt" IS NULL
            ORDER BY "Name";
            """;

        await using var connection = await _readDbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var organizations = (await connection.QueryAsync<OrganizationListingDto>(command)).AsList();
        
        return new GetAllOrganizationsQueryResult(organizations);
    }
}