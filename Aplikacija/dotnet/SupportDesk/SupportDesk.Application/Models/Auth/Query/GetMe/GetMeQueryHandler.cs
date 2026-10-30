using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Auth.Query.GetMe;

internal sealed class GetMeQueryHandler : AbstractQueryHandler<GetMeQuery, GetMeQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public GetMeQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext, 
        IDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetMeQueryResult> ExecuteAsync(GetMeQuery query, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.GetCurrentUserId();

        var userId = new UserId(currentUserId);

        const string sql = """
            SELECT "Id" AS "UserId", "UserName", "Email", "Role"
            FROM "Users"
            WHERE "Id" = @UserId
            LIMIT 1;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { UserId = currentUserId }, cancellationToken: cancellationToken);

        var row = await connection.QueryFirstOrDefaultAsync<UserDbRow>(command);
        if (row is null)
        {
            throw new ValidationException(UserErrors.NotFound(userId));
        }

        return new GetMeQueryResult(row.UserId, row.UserName, row.Email, (UserRole)row.Role);
    }

    private sealed record UserDbRow(Guid UserId, string UserName, string Email, short Role);
}