using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.Message.Events;

namespace SupportDesk.Application.Models.Messages.Event.MessageCreated;

internal sealed class SyncTicketReadModelOnMessageCreatedDomainEventHandler
    : IDomainEventHandler<MessageCreatedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnMessageCreatedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITicketReadModelService ticketReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(MessageCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "TicketId", "CreatedAt"
            FROM "Messages"
            WHERE "Id" = @MessageId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var message = await connection.QuerySingleOrDefaultAsync<MessageRow>(sql, new { MessageId = domainEvent.MessageId.IdValue });

        if (message is null)
        {
            return;
        }

        await _ticketReadModelService.UpdateTicketLastMessageAtAsync(
            message.TicketId,
            message.CreatedAt,
            cancellationToken);
    }

    private sealed record MessageRow
    {
        public Guid TicketId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
