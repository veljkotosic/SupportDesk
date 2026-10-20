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
                t."Id" AS "Id",
                o."Id" AS "OrganizationId",
                o."Name" AS "OrganizationName",
                c."Id" AS "CategoryId",
                c."Name" AS "CategoryName",
                customer."Id" AS "CustomerId",
                customer."UserName" AS "CustomerUserName",
                customer."Email" AS "CustomerEmail",
                agent."Id" AS "SupportAgentId",
                agent."UserName" AS "SupportAgentUserName",
                t."Status" AS "Status",
                t."Priority" AS "Priority",
                t."Feedback" AS "Feedback",
                t."Subject" AS "Subject",
                t."OpenedAt" AS "OpenedAt",
                t."AssignedAt" AS "AssignedAt",
                t."ClosedAt" AS "ClosedAt",
                t."LastMessageAt" AS "LastMessageAt"
            FROM "Tickets" t
            JOIN "Organizations" o ON o."Id" = t."OrganizationId"
            JOIN "Categories" c ON c."Id" = t."CategoryId"
            JOIN "Users" customer ON customer."Id" = t."CustomerId"
            LEFT JOIN "Users" agent ON agent."Id" = t."SupportAgentId"
            WHERE t."Id" = @TicketId;

            -- 2. Messages
            SELECT 
                m."Id" AS "Id",
                m."SenderId" AS "SenderId",
                sender."UserName" AS "SenderUserName",
                m."Text" AS "Text",
                m."CreatedAt" AS "CreatedAt"
            FROM "Messages" m
            JOIN "Users" sender ON m."SenderId" = sender."Id"
            WHERE m."TicketId" = @TicketId
            ORDER BY m."CreatedAt";

            -- 3. Notes
            SELECT 
                n."Id" AS "Id",
                n."AuthorId" AS "AuthorId",
                author."UserName" AS "AuthorUserName",
                n."Text" AS "Text",
                n."CreatedAt" AS "CreatedAt"
            FROM "Notes" n
            JOIN "Users" author ON n."AuthorId" = author."Id"
            WHERE n."TicketId" = @TicketId
            ORDER BY n."CreatedAt";
            """ : """
            -- 1. Ticket header
            SELECT 
                t."Id" AS "Id",
                o."Id" AS "OrganizationId",
                o."Name" AS "OrganizationName",
                c."Id" AS "CategoryId",
                c."Name" AS "CategoryName",
                customer."Id" AS "CustomerId",
                customer."UserName" AS "CustomerUserName",
                customer."Email" AS "CustomerEmail",
                agent."Id" AS "SupportAgentId",
                agent."UserName" AS "SupportAgentUserName",
                t."Status" AS "Status",
                t."Priority" AS "Priority",
                t."Feedback" AS "Feedback",
                t."Subject" AS "Subject",
                t."OpenedAt" AS "OpenedAt",
                t."AssignedAt" AS "AssignedAt",
                t."ClosedAt" AS "ClosedAt",
                t."LastMessageAt" AS "LastMessageAt"
            FROM "Tickets" t
            JOIN "Organizations" o ON o."Id" = t."OrganizationId"
            JOIN "Categories" c ON c."Id" = t."CategoryId"
            JOIN "Users" customer ON customer."Id" = t."CustomerId"
            LEFT JOIN "Users" agent ON agent."Id" = t."SupportAgentId"
            WHERE t."Id" = @TicketId;

            -- 2. Messages
            SELECT 
                m."Id" AS "Id",
                m."SenderId" AS "SenderId",
                sender."UserName" AS "SenderUserName",
                m."Text" AS "Text",
                m."CreatedAt" AS "CreatedAt"
            FROM "Messages" m
            JOIN "Users" sender ON m."SenderId" = sender."Id"
            WHERE m."TicketId" = @TicketId
            ORDER BY m."CreatedAt";
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