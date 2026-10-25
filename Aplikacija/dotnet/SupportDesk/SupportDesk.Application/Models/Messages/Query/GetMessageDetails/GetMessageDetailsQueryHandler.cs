using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Message.Validation;
using SupportDesk.Domain.Models.Message.ValueObjects;

namespace SupportDesk.Application.Models.Messages.Query.GetMessageDetails;

internal sealed class GetMessageDetailsQueryHandler
    : AbstractQueryHandler<GetMessageDetailsQuery, GetMessageDetailsQueryResult>
{
    private readonly IReadDbConnectionFactory _dbConnectionFactory;
    
    public GetMessageDetailsQueryHandler(
        PermissionChecker permissionChecker,
        IReadDbConnectionFactory dbConnectionFactory) 
        : base(permissionChecker)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetMessageDetailsQueryResult> ExecuteAsync(GetMessageDetailsQuery query, CancellationToken cancellationToken)
    {
        var messageId = new MessageId(query.MessageId);

        const string sql = """
            SELECT 
                m."Id",
                s."Id" AS "SenderId",
                s."UserName" AS "SenderUsername",
                m."Text",
                m."CreatedAt"
            FROM "Messages" m
            JOIN "Users" s ON m."SenderId" = s."Id"
            WHERE m."Id" = @MessageId
            LIMIT 1;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { MessageId = query.MessageId }, cancellationToken: cancellationToken);

        var result = await connection.QueryFirstOrDefaultAsync<GetMessageDetailsQueryResult>(command);

        if (result is null)
        {
            throw new ValidationException(MessageErrors.NotFound(messageId));
        }
        
        return result;
    }
}