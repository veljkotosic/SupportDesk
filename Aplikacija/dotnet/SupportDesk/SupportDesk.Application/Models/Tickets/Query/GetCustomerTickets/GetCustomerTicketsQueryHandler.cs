using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;
using SupportDesk.Application.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.TicketNotification.Enums;
using ElasticQuery = Elastic.Clients.Elasticsearch.QueryDsl.Query;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

internal sealed class GetCustomerTicketsQueryHandler
    : AbstractQueryHandler<GetCustomerTicketsQuery, GetCustomerTicketsQueryResult>
{
    private readonly IUserContext _userContext;
    private readonly ElasticsearchClient _client;
    private readonly ILogger<GetCustomerTicketsQueryHandler> _logger;

    public GetCustomerTicketsQueryHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ElasticsearchClient client,
        ILogger<GetCustomerTicketsQueryHandler> logger)
        : base(permissionChecker)
    {
        _userContext = userContext;
        _client = client;
        _logger = logger;
    }

    protected override async Task<GetCustomerTicketsQueryResult> ExecuteAsync(
        GetCustomerTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = _userContext.GetCurrentUserId();

        var postFilterQueries = new List<ElasticQuery>();

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
                postFilterQueries.Add(new BoolQuery
                {
                    Should = new ElasticQuery[]
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
                        new PrefixQuery(Infer.Field<TicketDocument>(f => f.Id))
                        {
                            Value = search.ToLowerInvariant()
                        }
                    },
                    MinimumShouldMatch = 1
                });
            }
        }

        var searchResponse = await _client.SearchAsync<TicketDocument>(s =>
        {
            s.Index(ElasticsearchIndices.Tickets)
             .Query(q => q.Bool(b => b.Filter(f => f.Term(t => t.Field(doc => doc.CustomerId).Value(customerId.ToString())))));

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
        }, cancellationToken).ConfigureAwait((false));

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
            (doc.UnreadNotifications ?? []).Select(n => new TicketNotificationDetailsDto(
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
}