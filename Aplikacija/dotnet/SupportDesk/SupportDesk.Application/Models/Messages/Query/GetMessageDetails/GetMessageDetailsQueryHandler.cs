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
    private readonly IReadDbConnectionFactory _readDbConnectionFactory;
    
    public GetMessageDetailsQueryHandler(
        PermissionChecker permissionChecker,
        IReadDbConnectionFactory readDbConnectionFactory) 
        : base(permissionChecker)
    {
        _readDbConnectionFactory = readDbConnectionFactory;
    }

    protected override async Task<GetMessageDetailsQueryResult> ExecuteAsync(GetMessageDetailsQuery query, CancellationToken cancellationToken)
    {
        var messageId = new MessageId(query.MessageId);

        const string sql = """
            SELECT 
                id AS "Id",
                sender_id AS "SenderId",
                sender_user_name AS "SenderUsername",
                text AS "Text",
                created_at AS "CreatedAt"
            FROM projections.ticket_messages
            WHERE id = @MessageId
            LIMIT 1;
            """;

        await using var connection = await _readDbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { MessageId = query.MessageId }, cancellationToken: cancellationToken);

        var result = await connection.QueryFirstOrDefaultAsync<GetMessageDetailsQueryResult>(command);

        if (result is null)
        {
            throw new ValidationException(MessageErrors.NotFound(messageId));
        }
        
        return result;
    }
}