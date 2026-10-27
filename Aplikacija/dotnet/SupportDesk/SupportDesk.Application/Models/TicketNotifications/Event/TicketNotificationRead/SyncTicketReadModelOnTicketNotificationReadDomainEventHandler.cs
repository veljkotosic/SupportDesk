using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.TicketNotification.Events;

namespace SupportDesk.Application.Models.TicketNotifications.Event.TicketNotificationRead;

internal sealed class SyncTicketReadModelOnTicketNotificationReadDomainEventHandler
    : IDomainEventHandler<TicketNotificationReadDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketNotificationReadDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITicketReadModelService ticketReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketNotificationReadDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "TicketId"
            FROM "TicketNotifications"
            WHERE "Id" = @NotificationId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var ticketId = await connection.QuerySingleOrDefaultAsync<Guid?>(sql, new { NotificationId = domainEvent.TicketNotificationId.IdValue });

        if (ticketId.HasValue)
        {
            await _ticketReadModelService.RemoveUnreadNotificationAsync(ticketId.Value, domainEvent.TicketNotificationId.IdValue, cancellationToken);
        }
    }
}
