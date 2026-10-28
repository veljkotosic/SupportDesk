using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Enums;
using SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;
using SupportDesk.Application.ReadModels;
using SupportDesk.Domain.Models.Ticket.Enums;
using ElasticQuery = Elastic.Clients.Elasticsearch.QueryDsl.Query;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery;

internal sealed class GetOrganizationTicketsQueryHandler
    : AbstractQueryHandler<GetOrganizationTicketsQuery, GetOrganizationTicketsQueryResult>
{
    private readonly ITenantContext _tenantContext;
    private readonly ElasticsearchClient _client;
    private readonly ILogger<GetOrganizationTicketsQueryHandler> _logger;

    public GetOrganizationTicketsQueryHandler(
        PermissionChecker permissionChecker,
        ITenantContext tenantContext,
        ElasticsearchClient client,
        ILogger<GetOrganizationTicketsQueryHandler> logger)
        : base(permissionChecker)
    {
        _tenantContext = tenantContext;
        _client = client;
        _logger = logger;
    }

    protected override async Task<GetOrganizationTicketsQueryResult> ExecuteAsync(
        GetOrganizationTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;

        var postFilterQueries = new List<ElasticQuery>();

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
                    Should = new ElasticQuery[]
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
                        so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Asc, Missing = "_last" });
                        so.Field(f => f.Id, new FieldSort { Order = SortOrder.Asc });
                        break;
                    case TicketSortOption.Priority:
                        so.Field(f => f.Priority, new FieldSort { Order = SortOrder.Desc });
                        so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" });
                        so.Field(f => f.Id, new FieldSort { Order = SortOrder.Desc });
                        break;
                    case TicketSortOption.Latest:
                    default:
                        so.Field(f => f.LastMessageAt, new FieldSort { Order = SortOrder.Desc, Missing = "_last" });
                        so.Field(f => f.Id, new FieldSort { Order = SortOrder.Desc });
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
}