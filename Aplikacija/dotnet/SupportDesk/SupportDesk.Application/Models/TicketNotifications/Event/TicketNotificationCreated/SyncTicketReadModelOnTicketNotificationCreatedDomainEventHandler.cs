using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.TicketNotification.Events;

namespace SupportDesk.Application.Models.TicketNotifications.Event.TicketNotificationCreated;

internal sealed class SyncTicketReadModelOnTicketNotificationCreatedDomainEventHandler
    : IDomainEventHandler<TicketNotificationCreatedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketNotificationCreatedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITicketReadModelService ticketReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketNotificationCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "Id", "OrganizationId", "TicketId", "Text", "Status", "CreatedAt"
            FROM "TicketNotifications"
            WHERE "Id" = @NotificationId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<NotificationRow>(sql, new { NotificationId = domainEvent.TicketNotificationId.IdValue });

        if (row is null)
        {
            return;
        }

        var dto = new TicketNotificationDetailsDto(
            row.Id,
            row.OrganizationId,
            row.TicketId,
            row.Text,
            row.Status,
            row.CreatedAt);

        await _ticketReadModelService.AddUnreadNotificationAsync(row.TicketId, dto, cancellationToken);
    }

    private sealed record NotificationRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; } = null!;
        public TicketNotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
