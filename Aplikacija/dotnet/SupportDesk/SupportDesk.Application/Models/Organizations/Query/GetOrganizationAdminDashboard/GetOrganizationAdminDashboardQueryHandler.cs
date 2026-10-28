using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;
using SupportDesk.Application.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

internal sealed class GetOrganizationAdminDashboardQueryHandler
    : AbstractQueryHandler<GetOrganizationAdminDashboardQuery, GetOrganizationAdminDashboardQueryResult>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly ElasticsearchClient _client;
    private readonly ILogger<GetOrganizationAdminDashboardQueryHandler> _logger;

    public GetOrganizationAdminDashboardQueryHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        ElasticsearchClient client,
        ILogger<GetOrganizationAdminDashboardQueryHandler> logger)
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _client = client;
        _logger = logger;
    }

    protected override async Task<GetOrganizationAdminDashboardQueryResult> ExecuteAsync(
        GetOrganizationAdminDashboardQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var startDateUtc = nowUtc.Date.AddDays(-6);

        // 1. Get Organization Name
        var orgDocResponse = await _client.GetAsync<OrganizationDocument>(
            ElasticsearchIndices.Organizations,
            organizationId.ToString(),
            cancellationToken);

        var organizationName = orgDocResponse.Source?.Name ?? string.Empty;

        // 2. Query all support agents for this organization from users index
        var agentsResponse = await _client.SearchAsync<UserDocument>(s => s
            .Index(ElasticsearchIndices.Users)
            .Query(q => q.Bool(b => b.Filter(
                f => f.Term(t => t.Field(u => u.OrganizationId).Value(organizationId.ToString())),
                f => f.Term(t => t.Field(u => u.Role).Value((short)UserRole.SupportAgent))
            )))
            .Size(1000)
            .Sort(so => so.Field(f => f.UserName, new FieldSort { Order = SortOrder.Asc })), cancellationToken);

        var agentsList = agentsResponse.IsValidResponse ? agentsResponse.Documents.ToList() : [];

        // 3. Query tickets index: 6 recent tickets + aggregations
        var ticketsResponse = await _client.SearchAsync<TicketDocument>(s => s
            .Index(ElasticsearchIndices.Tickets)
            .Query(q => q.Term(t => t.Field(f => f.OrganizationId).Value(organizationId.ToString())))
            .Size(6)
            .Sort(so =>
            {
                so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" });
                so.Field(f => f.Id, new FieldSort { Order = SortOrder.Desc });
            })
            .Aggregations(a => a
                .Add("open_tickets", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Open))))
                .Add("assigned_tickets", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Assigned))))
                .Add("closed_tickets", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Closed))))
                .Add("agent_assigned_counts", agg => agg
                    .Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Assigned)))
                    .Aggregations(sub => sub.Add("by_agent", t => t.Terms(terms => terms.Field(doc => doc.SupportAgentId).Size(1000)))))
                .Add("opened_daily", agg => agg
                    .Filter(q => q.Range(r => r.DateRange(dr => dr.Field(doc => doc.OpenedAt).Gte(startDateUtc))))
                    .Aggregations(sub => sub.Add("histogram", h => h.DateHistogram(dh => dh.Field(doc => doc.OpenedAt).CalendarInterval(CalendarInterval.Day).MinDocCount(0)))))
                .Add("closed_daily", agg => agg
                    .Filter(q => q.Bool(b => b.Must(
                        m => m.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Closed)),
                        m => m.Range(r => r.DateRange(dr => dr.Field(doc => doc.ClosedAt).Gte(startDateUtc)))
                    )))
                    .Aggregations(sub => sub.Add("histogram", h => h.DateHistogram(dh => dh.Field(doc => doc.ClosedAt).CalendarInterval(CalendarInterval.Day).MinDocCount(0)))))
            ), cancellationToken);

        if (!ticketsResponse.IsValidResponse)
        {
            _logger.LogError("Error in GetOrganizationAdminDashboard search: {Reason}", ticketsResponse.DebugInformation);
        }

        var openTicketsCount = (int)(ticketsResponse.Aggregations?.GetFilter("open_tickets")?.DocCount ?? 0);
        var assignedTicketsCount = (int)(ticketsResponse.Aggregations?.GetFilter("assigned_tickets")?.DocCount ?? 0);
        var closedTicketsCount = (int)(ticketsResponse.Aggregations?.GetFilter("closed_tickets")?.DocCount ?? 0);

        var summary = new OrganizationAdminDashboardSummaryDto(
            openTicketsCount,
            assignedTicketsCount,
            closedTicketsCount,
            agentsList.Count);

        // Map assigned ticket counts per agent
        var filterAgg = ticketsResponse.Aggregations?.GetFilter("agent_assigned_counts");
        var agentBuckets = filterAgg?.Aggregations?.GetStringTerms("by_agent")?.Buckets;

        var assignedCountsByAgentId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (agentBuckets != null)
        {
            foreach (var b in agentBuckets)
            {
                var key = b.Key.ToString() ?? string.Empty;
                assignedCountsByAgentId[key] = (int)b.DocCount;
            }
        }

        var agents = agentsList.Select(agent => new OrganizationAdminDashboardAgentDto(
            agent.Id,
            agent.UserName,
            assignedCountsByAgentId.GetValueOrDefault(agent.Id.ToString(), 0)
        )).OrderBy(ag => ag.UserName).ToList();

        // Map 6 recent tickets
        var recentTickets = (ticketsResponse.IsValidResponse ? ticketsResponse.Documents : Enumerable.Empty<TicketDocument>())
            .Select(doc => new DashboardTicketDetailsDto(
                doc.Id,
                doc.OrganizationId,
                doc.OrganizationName,
                doc.CategoryId,
                doc.CategoryName,
                doc.CustomerId,
                doc.CustomerUserName,
                doc.SupportAgentId,
                doc.SupportAgentUserName,
                (TicketStatus)doc.Status,
                (TicketPriority)doc.Priority,
                doc.Subject,
                doc.OpenedAt,
                doc.AssignedAt,
                doc.ClosedAt,
                (TicketFeedback)doc.Feedback,
                doc.LastMessageAt,
                (doc.UnreadNotifications ?? []).Select(n => new TicketNotificationDetailsDto(
                    n.Id,
                    n.OrganizationId,
                    n.TicketId,
                    n.Text,
                    (TicketNotificationStatus)n.Status,
                    n.CreatedAt)).ToList()
            )).ToList();

        // Map 7-day ticket volume trend
        var openedDailyBuckets = ticketsResponse.Aggregations?
            .GetFilter("opened_daily")?
            .Aggregations?.GetDateHistogram("histogram")?
            .Buckets;

        var closedDailyBuckets = ticketsResponse.Aggregations?
            .GetFilter("closed_daily")?
            .Aggregations?.GetDateHistogram("histogram")?
            .Buckets;

        var openedByDate = new Dictionary<DateOnly, int>();
        if (openedDailyBuckets != null)
        {
            foreach (var b in openedDailyBuckets)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(b.Key).UtcDateTime;
                openedByDate[DateOnly.FromDateTime(dt)] = (int)b.DocCount;
            }
        }

        var closedByDate = new Dictionary<DateOnly, int>();
        if (closedDailyBuckets != null)
        {
            foreach (var b in closedDailyBuckets)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(b.Key).UtcDateTime;
                closedByDate[DateOnly.FromDateTime(dt)] = (int)b.DocCount;
            }
        }

        var ticketVolume = new List<OrganizationAdminDashboardTicketVolumeEntryDto>();
        for (var day = startDateUtc.Date; day <= nowUtc.Date; day = day.AddDays(1))
        {
            var dateOnly = DateOnly.FromDateTime(day);
            ticketVolume.Add(new OrganizationAdminDashboardTicketVolumeEntryDto(
                dateOnly,
                openedByDate.GetValueOrDefault(dateOnly, 0),
                closedByDate.GetValueOrDefault(dateOnly, 0)
            ));
        }

        return new GetOrganizationAdminDashboardQueryResult(
            organizationName,
            summary,
            ticketVolume,
            agents,
            recentTickets);
    }
}
