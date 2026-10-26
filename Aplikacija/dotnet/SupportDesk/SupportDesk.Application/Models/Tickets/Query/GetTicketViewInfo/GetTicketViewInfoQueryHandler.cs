using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo;

internal sealed class GetTicketViewInfoQueryHandler
    : AbstractQueryHandler<GetTicketViewInfoQuery, GetTicketViewInfoQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetTicketViewInfoQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITenantContext tenantContext,
        IDbConnectionFactory dbConnectionFactory)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetTicketViewInfoQueryResult> ExecuteAsync(
        GetTicketViewInfoQuery query,
        CancellationToken cancellationToken)
    {
        var ticketId = query.TicketId;
        var currentUserId = _userContext.GetCurrentUserId();
        var organizationId = _tenantContext.GetCurrentOrganizationId();
        var isOrganizationMember = organizationId != null;

        var sql = isOrganizationMember ? """
            -- 1. Ticket header
            SELECT 
                id AS "Id",
                organization_id AS "OrganizationId",
                organization_name AS "OrganizationName",
                category_id AS "CategoryId",
                category_name AS "CategoryName",
                customer_id AS "CustomerId",
                customer_user_name AS "CustomerUserName",
                customer_email AS "CustomerEmail",
                support_agent_id AS "SupportAgentId",
                support_agent_user_name AS "SupportAgentUserName",
                status AS "Status",
                priority AS "Priority",
                feedback AS "Feedback",
                subject AS "Subject",
                opened_at AS "OpenedAt",
                assigned_at AS "AssignedAt",
                closed_at AS "ClosedAt",
                last_message_at AS "LastMessageAt"
            FROM projections.ticket_summaries
            WHERE id = @TicketId;

            -- 2. Messages
            SELECT 
                id AS "Id",
                sender_id AS "SenderId",
                sender_user_name AS "SenderUserName",
                text AS "Text",
                created_at AS "CreatedAt"
            FROM projections.ticket_messages
            WHERE ticket_id = @TicketId
            ORDER BY created_at;

            -- 3. Notes
            SELECT 
                id AS "Id",
                author_id AS "AuthorId",
                author_user_name AS "AuthorUserName",
                text AS "Text",
                created_at AS "CreatedAt"
            FROM projections.ticket_notes
            WHERE ticket_id = @TicketId
            ORDER BY created_at;
            """ : """
            -- 1. Ticket header
            SELECT 
                id AS "Id",
                organization_id AS "OrganizationId",
                organization_name AS "OrganizationName",
                category_id AS "CategoryId",
                category_name AS "CategoryName",
                customer_id AS "CustomerId",
                customer_user_name AS "CustomerUserName",
                customer_email AS "CustomerEmail",
                support_agent_id AS "SupportAgentId",
                support_agent_user_name AS "SupportAgentUserName",
                status AS "Status",
                priority AS "Priority",
                feedback AS "Feedback",
                subject AS "Subject",
                opened_at AS "OpenedAt",
                assigned_at AS "AssignedAt",
                closed_at AS "ClosedAt",
                last_message_at AS "LastMessageAt"
            FROM projections.ticket_summaries
            WHERE id = @TicketId;

            -- 2. Messages
            SELECT 
                id AS "Id",
                sender_id AS "SenderId",
                sender_user_name AS "SenderUserName",
                text AS "Text",
                created_at AS "CreatedAt"
            FROM projections.ticket_messages
            WHERE ticket_id = @TicketId
            ORDER BY created_at;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { TicketId = ticketId },
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var header = await multi.ReadFirstOrDefaultAsync<RawTicketHeaderRow>();

        if (header is null || (!isOrganizationMember && header.CustomerId != currentUserId))
        {
            throw new ValidationException(TicketErrors.NotFound(new TicketId(ticketId)));
        }

        var messages = (await multi.ReadAsync<TicketMessageDetailsDto>()).ToList();
        var notes = isOrganizationMember
            ? (await multi.ReadAsync<TicketNoteDetailsDto>()).ToList()
            : [];

        var ticketViewInfo = new TicketViewInfoDto(
            header.Id,
            header.OrganizationId,
            header.OrganizationName,
            header.CategoryId,
            header.CategoryName,
            header.CustomerId,
            header.CustomerUserName,
            header.CustomerEmail,
            header.SupportAgentId,
            header.SupportAgentUserName,
            header.Status,
            header.Priority,
            header.Feedback,
            header.Subject,
            header.OpenedAt,
            header.AssignedAt,
            header.ClosedAt,
            header.LastMessageAt,
            messages,
            notes);

        return new GetTicketViewInfoQueryResult(ticketViewInfo);
    }

    private sealed record RawTicketHeaderRow
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