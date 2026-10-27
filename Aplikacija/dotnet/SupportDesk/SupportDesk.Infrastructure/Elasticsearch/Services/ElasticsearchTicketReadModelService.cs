using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;
using SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;
using SupportDesk.Application.Models.Tickets.Enums;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Infrastructure.Elasticsearch.Documents;

namespace SupportDesk.Infrastructure.Elasticsearch.Services;

public sealed class ElasticsearchTicketReadModelService : ITicketReadModelService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchTicketReadModelService> _logger;

    public ElasticsearchTicketReadModelService(
        ElasticsearchClient client,
        ILogger<ElasticsearchTicketReadModelService> logger)
    {
        _client = client;
        _logger = logger;
    }

    #region Projections

    public async Task IndexTicketAsync(TicketReadModelDto ticket, CancellationToken cancellationToken)
    {
        var doc = new TicketDocument
        {
            Id = ticket.Id,
            OrganizationId = ticket.OrganizationId,
            OrganizationName = ticket.OrganizationName,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.CategoryName,
            CustomerId = ticket.CustomerId,
            CustomerUserName = ticket.CustomerUserName,
            CustomerEmail = ticket.CustomerEmail,
            SupportAgentId = ticket.SupportAgentId,
            SupportAgentUserName = ticket.SupportAgentUserName,
            Status = (short)ticket.Status,
            Priority = (short)ticket.Priority,
            Feedback = (short)ticket.Feedback,
            Subject = ticket.Subject,
            OpenedAt = ticket.OpenedAt,
            AssignedAt = ticket.AssignedAt,
            ClosedAt = ticket.ClosedAt,
            LastMessageAt = ticket.LastMessageAt,
            UnreadNotifications = ticket.UnreadNotifications.Select(n => new TicketNotificationDocument
            {
                Id = n.Id,
                OrganizationId = n.OrganizationId,
                TicketId = n.TicketId,
                Text = n.Text,
                Status = (short)n.Status,
                CreatedAt = n.CreatedAt
            }).ToList()
        };

        var response = await _client.IndexAsync(doc, idx => idx
            .Index(ElasticsearchIndices.Tickets)
            .Id(doc.Id.ToString()), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index ticket {TicketId}: {Reason}", ticket.Id, response.DebugInformation);
        }
    }

    public async Task UpdateTicketAssignmentAsync(
        Guid ticketId,
        Guid? supportAgentId,
        string? supportAgentUserName,
        TicketStatus status,
        DateTime? assignedAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                SupportAgentId = supportAgentId,
                SupportAgentUserName = supportAgentUserName,
                Status = (short)status,
                AssignedAt = assignedAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket assignment {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketStatusAsync(
        Guid ticketId,
        TicketStatus status,
        DateTime? closedAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                Status = (short)status,
                ClosedAt = closedAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket status {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketFeedbackAsync(
        Guid ticketId,
        TicketFeedback feedback,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                Feedback = (short)feedback
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket feedback {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task UpdateTicketLastMessageAtAsync(
        Guid ticketId,
        DateTime lastMessageAt,
        CancellationToken cancellationToken)
    {
        var updateResponse = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Doc(new
            {
                LastMessageAt = lastMessageAt
            }), cancellationToken);

        if (!updateResponse.IsValidResponse)
        {
            _logger.LogError("Failed to update ticket last message {TicketId}: {Reason}", ticketId, updateResponse.DebugInformation);
        }
    }

    public async Task AddUnreadNotificationAsync(
        Guid ticketId,
        TicketNotificationDetailsDto notification,
        CancellationToken cancellationToken)
    {
        var script = """
            if (ctx._source.unreadNotifications == null) {
                ctx._source.unreadNotifications = new ArrayList();
            }
            ctx._source.unreadNotifications.add(params.notification);
            """;

        var response = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Script(s => s
                .Source(script)
                .Params(p => p.Add("notification", new
                {
                    id = notification.Id,
                    organizationId = notification.OrganizationId,
                    ticketId = notification.TicketId,
                    text = notification.Text,
                    status = (short)notification.Status,
                    createdAt = notification.CreatedAt
                }))), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to add unread notification to ticket {TicketId}: {Reason}", ticketId, response.DebugInformation);
        }
    }

    public async Task RemoveUnreadNotificationAsync(
        Guid ticketId,
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var script = """
            if (ctx._source.unreadNotifications != null) {
                ctx._source.unreadNotifications.removeIf(n -> n.id == params.notificationId);
            }
            """;

        var response = await _client.UpdateAsync<TicketDocument, object>(
            ElasticsearchIndices.Tickets,
            ticketId.ToString(),
            u => u.Script(s => s
                .Source(script)
                .Params(p => p.Add("notificationId", notificationId.ToString()))), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to remove unread notification {NotificationId} from ticket {TicketId}: {Reason}", notificationId, ticketId, response.DebugInformation);
        }
    }

    #endregion

    #region Queries

    public async Task<GetCustomerTicketsQueryResult> GetCustomerTicketsAsync(
        GetCustomerTicketsQuery query,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var postFilterQueries = new List<Query>();

        if (query.Status.HasValue)
        {
            postFilterQueries.Add(new TermQuery(Infer.Field<TicketDocument>(f => f.Status))
            {
                Value = (short)query.Status.Value
            });
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim();
            if (Guid.TryParse(search, out var searchGuid))
            {
                postFilterQueries.Add(new TermQuery(Infer.Field<TicketDocument>(f => f.Id))
                {
                    Value = searchGuid.ToString()
                });
            }
            else
            {
                var searchPattern = $"*{search.ToLowerInvariant()}*";

                postFilterQueries.Add(new BoolQuery
                {
                    Should = new Query[]
                    {
                        new MultiMatchQuery
                        {
                            Query = search,
                            Fields = new[]
                            {
                                Infer.Field<TicketDocument>(f => f.Subject),
                                Infer.Field<TicketDocument>(f => f.OrganizationName),
                                Infer.Field<TicketDocument>(f => f.CategoryName)
                            },
                            Type = TextQueryType.PhrasePrefix
                        },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.Subject)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.OrganizationName)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.CategoryName)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.Id)) { Value = searchPattern }
                    },
                    MinimumShouldMatch = 1
                });
            }
        }

        var searchResponse = await _client.SearchAsync<TicketDocument>(s =>
        {
            s.Index(ElasticsearchIndices.Tickets)
             .Query(q => q.Term(t => t.Field(f => f.CustomerId).Value(customerId.ToString())));

            if (postFilterQueries.Count > 0)
            {
                s.PostFilter(pf => pf.Bool(b => b.Filter(postFilterQueries.ToArray())));
            }

            s.Sort(so => so
                .Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" })
                .Field(f => f.Id, new FieldSort { Order = SortOrder.Desc }))
            .From(query.Skip)
            .Size(query.Take)
            .Aggregations(a => a
                .Add("open_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Open))))
                .Add("assigned_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Assigned))))
                .Add("closed_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Closed))))
            );
        }, cancellationToken);

        if (!searchResponse.IsValidResponse)
        {
            _logger.LogError("Error in GetCustomerTickets search: {Reason}", searchResponse.DebugInformation);
            return new GetCustomerTicketsQueryResult([], 0, 0, 0, 0, 0);
        }

        var openCount = (int)(searchResponse.Aggregations.GetFilter("open_count")?.DocCount ?? 0);
        var assignedCount = (int)(searchResponse.Aggregations.GetFilter("assigned_count")?.DocCount ?? 0);
        var closedCount = (int)(searchResponse.Aggregations.GetFilter("closed_count")?.DocCount ?? 0);
        var totalCustomerTickets = openCount + assignedCount + closedCount;

        var totalCount = (int)searchResponse.Total;

        var tickets = searchResponse.Documents.Select(doc => new CustomerTicketListingDto(
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
            (TicketFeedback)doc.Feedback,
            doc.Subject,
            doc.OpenedAt,
            doc.AssignedAt,
            doc.ClosedAt,
            doc.LastMessageAt,
            doc.UnreadNotifications.Select(n => new TicketNotificationDetailsDto(
                n.Id,
                n.OrganizationId,
                n.TicketId,
                n.Text,
                (TicketNotificationStatus)n.Status,
                n.CreatedAt)).ToList()
        )).ToList();

        return new GetCustomerTicketsQueryResult(
            tickets,
            totalCount,
            totalCustomerTickets,
            openCount,
            assignedCount,
            closedCount);
    }

    public async Task<GetOrganizationTicketsQueryResult> GetOrganizationTicketsAsync(
        GetOrganizationTicketsQuery query,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var postFilterQueries = new List<Query>();

        if (query.Status.HasValue)
        {
            postFilterQueries.Add(new TermQuery(Infer.Field<TicketDocument>(f => f.Status))
            {
                Value = (short)query.Status.Value
            });
        }

        if (query.Priority.HasValue)
        {
            postFilterQueries.Add(new TermQuery(Infer.Field<TicketDocument>(f => f.Priority))
            {
                Value = (short)query.Priority.Value
            });
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim();
            if (Guid.TryParse(search, out var searchGuid))
            {
                postFilterQueries.Add(new TermQuery(Infer.Field<TicketDocument>(f => f.Id))
                {
                    Value = searchGuid.ToString()
                });
            }
            else
            {
                var searchPattern = $"*{search.ToLowerInvariant()}*";

                postFilterQueries.Add(new BoolQuery
                {
                    Should = new Query[]
                    {
                        new MultiMatchQuery
                        {
                            Query = search,
                            Fields = new[]
                            {
                                Infer.Field<TicketDocument>(f => f.Subject),
                                Infer.Field<TicketDocument>(f => f.CustomerUserName),
                                Infer.Field<TicketDocument>(f => f.CategoryName)
                            },
                            Type = TextQueryType.PhrasePrefix
                        },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.Subject)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.CustomerUserName)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.CategoryName)) { Value = searchPattern },
                        new WildcardQuery(Infer.Field<TicketDocument>(f => f.Id)) { Value = searchPattern }
                    },
                    MinimumShouldMatch = 1
                });
            }
        }

        var searchResponse = await _client.SearchAsync<TicketDocument>(s =>
        {
            s.Index(ElasticsearchIndices.Tickets)
             .Query(q => q.Term(t => t.Field(f => f.OrganizationId).Value(organizationId.ToString())));

            if (postFilterQueries.Count > 0)
            {
                s.PostFilter(pf => pf.Bool(b => b.Filter(postFilterQueries.ToArray())));
            }

            s.Sort(so =>
            {
                switch (query.SortBy)
                {
                    case TicketSortOption.Oldest:
                        so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Asc, Missing = "_last" })
                          .Field(f => f.Id, new FieldSort { Order = SortOrder.Asc });
                        break;
                    case TicketSortOption.Priority:
                        so.Field(f => f.Priority, new FieldSort { Order = SortOrder.Desc })
                          .Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" })
                          .Field(f => f.Id, new FieldSort { Order = SortOrder.Desc });
                        break;
                    case TicketSortOption.Latest:
                    default:
                        so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" })
                          .Field(f => f.Id, new FieldSort { Order = SortOrder.Desc });
                        break;
                }
            })
            .From(query.Skip)
            .Size(query.Take)
            .Aggregations(a => a
                .Add("open_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Open))))
                .Add("assigned_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Assigned))))
                .Add("closed_count", agg => agg.Filter(q => q.Term(t => t.Field(doc => doc.Status).Value((short)TicketStatus.Closed))))
            );
        }, cancellationToken);

        if (!searchResponse.IsValidResponse)
        {
            _logger.LogError("Error in GetOrganizationTickets search: {Reason}", searchResponse.DebugInformation);
            return new GetOrganizationTicketsQueryResult([], 0, 0, 0, 0, 0);
        }

        var openCount = (int)(searchResponse.Aggregations.GetFilter("open_count")?.DocCount ?? 0);
        var assignedCount = (int)(searchResponse.Aggregations.GetFilter("assigned_count")?.DocCount ?? 0);
        var closedCount = (int)(searchResponse.Aggregations.GetFilter("closed_count")?.DocCount ?? 0);
        var allCount = openCount + assignedCount + closedCount;

        var totalCount = (int)searchResponse.Total;

        var tickets = searchResponse.Documents.Select(doc => new OrganizationTicketListingDto(
            doc.Id,
            doc.CategoryId,
            doc.CategoryName,
            doc.CustomerId,
            doc.CustomerUserName,
            doc.CustomerEmail,
            doc.SupportAgentId,
            doc.SupportAgentUserName,
            (TicketStatus)doc.Status,
            (TicketPriority)doc.Priority,
            (TicketFeedback)doc.Feedback,
            doc.Subject,
            doc.LastMessageAt
        )).ToList();

        return new GetOrganizationTicketsQueryResult(
            tickets,
            totalCount,
            allCount,
            openCount,
            assignedCount,
            closedCount);
    }

    public async Task<GetOrganizationAdminDashboardQueryResult> GetOrganizationAdminDashboardAsync(
        GetOrganizationAdminDashboardQuery query,
        Guid organizationId,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var startDateUtc = nowUtc.Date.AddDays(-6);
        var endDateUtc = nowUtc.Date.AddDays(1);

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
            .Sort(so => so
                .Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" })
                .Field(f => f.Id, new FieldSort { Order = SortOrder.Desc }))
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

    #endregion
}
