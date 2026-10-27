using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketAssigned;

internal sealed class SyncTicketReadModelOnTicketAssignedDomainEventHandler
    : IDomainEventHandler<TicketAssignedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketAssignedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITicketReadModelService ticketReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT "UserName"
            FROM "Users"
            WHERE "Id" = @UserId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var agentUserName = await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = domainEvent.SupportAgentId.IdValue });

        await _ticketReadModelService.UpdateTicketAssignmentAsync(
            domainEvent.TicketId.IdValue,
            domainEvent.SupportAgentId.IdValue,
            agentUserName,
            TicketStatus.Assigned,
            DateTime.UtcNow,
            cancellationToken);
    }
}
