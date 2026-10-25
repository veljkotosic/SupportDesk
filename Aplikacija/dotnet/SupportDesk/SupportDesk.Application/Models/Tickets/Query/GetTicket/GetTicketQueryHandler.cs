using Dapper;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicket;

internal sealed class GetTicketQueryHandler
    : AbstractQueryHandler<GetTicketQuery, GetTicketQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    private readonly IReadDbConnectionFactory _dbConnectionFactory;

    public GetTicketQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITenantContext tenantContext,
        IReadDbConnectionFactory dbConnectionFactory)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    protected override async Task<GetTicketQueryResult> ExecuteAsync(
        GetTicketQuery query,
        CancellationToken cancellationToken)
    {
        var ticketId = query.TicketId;
        var currentUserId = _userContext.GetCurrentUserId();
        var organizationId = _tenantContext.GetCurrentOrganizationId();

        const string sql = """
            -- 1. Ticket details
            SELECT 
                t."Id" AS "Id",
                o."Id" AS "OrganizationId",
                o."Name" AS "OrganizationName",
                c."Id" AS "CategoryId",
                c."Name" AS "CategoryName",
                customer."Id" AS "CustomerId",
                customer."UserName" AS "CustomerUserName",
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

            -- 2. Unread notifications
            SELECT 
                n."Id" AS "Id",
                n."OrganizationId" AS "OrganizationId",
                n."TicketId" AS "TicketId",
                n."Text" AS "Text",
                n."Status" AS "Status",
                n."CreatedAt" AS "CreatedAt"
            FROM "TicketNotifications" n
            WHERE n."TicketId" = @TicketId
              AND n."Status" = @UnreadStatus;
            """;

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            new { TicketId = ticketId, UnreadStatus = (short)TicketNotificationStatus.Unread },
            cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);

        var rawTicket = await multi.ReadFirstOrDefaultAsync<RawTicketRow>();

        if (rawTicket is null || (organizationId is null && rawTicket.CustomerId != currentUserId))
        {
            throw new ValidationException(TicketErrors.NotFound(new TicketId(ticketId)));
        }

        var rawNotifications = (await multi.ReadAsync<RawNotificationRow>()).ToList();
        var notifications = rawNotifications.Select(n => new TicketNotificationDetailsDto(
            n.Id,
            n.OrganizationId,
            n.TicketId,
            n.Text,
            n.Status,
            n.CreatedAt)).ToList();

        var details = new DashboardTicketDetailsDto(
            rawTicket.Id,
            rawTicket.OrganizationId,
            rawTicket.OrganizationName,
            rawTicket.CategoryId,
            rawTicket.CategoryName,
            rawTicket.CustomerId,
            rawTicket.CustomerUserName,
            rawTicket.SupportAgentId,
            rawTicket.SupportAgentUserName,
            rawTicket.Status,
            rawTicket.Priority,
            rawTicket.Subject,
            rawTicket.OpenedAt,
            rawTicket.AssignedAt,
            rawTicket.ClosedAt,
            rawTicket.Feedback,
            rawTicket.LastMessageAt,
            notifications);

        return new GetTicketQueryResult(details);
    }

    private sealed record RawTicketRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string CustomerUserName { get; set; } = null!;
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

    private sealed record RawNotificationRow
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TicketId { get; set; }
        public string Text { get; set; } = null!;
        public TicketNotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}