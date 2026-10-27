using Dapper;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Events;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Tickets.Event.TicketOpened;

internal sealed class SyncTicketReadModelOnTicketOpenedDomainEventHandler
    : IDomainEventHandler<TicketOpenedDomainEvent>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITicketReadModelService _ticketReadModelService;

    public SyncTicketReadModelOnTicketOpenedDomainEventHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITicketReadModelService ticketReadModelService)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _ticketReadModelService = ticketReadModelService;
    }

    public async Task HandleAsync(TicketOpenedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 
                t."Id", t."OrganizationId", o."Name" AS "OrganizationName",
                t."CategoryId", c."Name" AS "CategoryName",
                t."CustomerId", cu."UserName" AS "CustomerUserName", cu."Email" AS "CustomerEmail",
                t."SupportAgentId", sa."UserName" AS "SupportAgentUserName",
                t."Status", t."Priority", t."Feedback", t."Subject",
                t."OpenedAt", t."AssignedAt", t."ClosedAt", t."LastMessageAt"
            FROM "Tickets" t
            JOIN "Organizations" o ON o."Id" = t."OrganizationId"
            JOIN "Categories" c ON c."Id" = t."CategoryId"
            JOIN "Users" cu ON cu."Id" = t."CustomerId"
            LEFT JOIN "Users" sa ON sa."Id" = t."SupportAgentId"
            WHERE t."Id" = @TicketId;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<TicketRow>(sql, new { TicketId = domainEvent.TicketId.IdValue });

        if (row is null)
        {
            return;
        }

        var dto = new TicketReadModelDto(
            row.Id,
            row.OrganizationId,
            row.OrganizationName,
            row.CategoryId,
            row.CategoryName,
            row.CustomerId,
            row.CustomerUserName,
            row.CustomerEmail,
            row.SupportAgentId,
            row.SupportAgentUserName,
            row.Status,
            row.Priority,
            row.Feedback,
            row.Subject,
            row.OpenedAt,
            row.AssignedAt,
            row.ClosedAt,
            row.LastMessageAt,
            []
        );

        await _ticketReadModelService.IndexTicketAsync(dto, cancellationToken);
    }

    private sealed record TicketRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerUserName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public Guid? SupportAgentId { get; set; }
        public string? SupportAgentUserName { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketFeedback Feedback { get; set; }
        public string Subject { get; set; } = null!;
        public DateTime OpenedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
