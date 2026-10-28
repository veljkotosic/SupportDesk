using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.ReadModels;

namespace SupportDesk.Infrastructure.Elasticsearch.Initialization;

public sealed class ElasticsearchIndexInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchIndexInitializer> _logger;

    public ElasticsearchIndexInitializer(
        ElasticsearchClient client,
        ILogger<ElasticsearchIndexInitializer> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task EnsureIndicesCreatedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureTicketsIndexAsync(cancellationToken);
        await EnsureUsersIndexAsync(cancellationToken);
        await EnsureOrganizationsIndexAsync(cancellationToken);
    }

    private async Task EnsureTicketsIndexAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ElasticsearchIndices.Tickets, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        _logger.LogInformation("Creating Elasticsearch index '{Index}'...", ElasticsearchIndices.Tickets);

        var createResponse = await _client.Indices.CreateAsync(ElasticsearchIndices.Tickets, c => c
            .Mappings(m => m
                .Properties<TicketDocument>(p => p
                    .Keyword(k => k.Id)
                    .Keyword(k => k.OrganizationId)
                    .Text(t => t.OrganizationName, t => t.Fields(f => f.Keyword("keyword")))
                    .Keyword(k => k.CategoryId)
                    .Text(t => t.CategoryName, t => t.Fields(f => f.Keyword("keyword")))
                    .Keyword(k => k.CustomerId)
                    .Text(t => t.CustomerUserName, t => t.Fields(f => f.Keyword("keyword")))
                    .Keyword(k => k.CustomerEmail)
                    .Keyword(k => k.SupportAgentId)
                    .Text(t => t.SupportAgentUserName, t => t.Fields(f => f.Keyword("keyword")))
                    .ShortNumber(s => s.Status)
                    .ShortNumber(s => s.Priority)
                    .ShortNumber(s => s.Feedback)
                    .Text(t => t.Subject, t => t.Fields(f => f.Keyword("keyword")))
                    .Date(d => d.OpenedAt)
                    .Date(d => d.AssignedAt)
                    .Date(d => d.ClosedAt)
                    .Date(d => d.LastMessageAt)
                    .Nested(n => n.UnreadNotifications, np => np
                        .Properties(pp => pp
                            .Keyword("id")
                            .Keyword("organizationId")
                            .Keyword("ticketId")
                            .Text("text")
                            .ShortNumber("status")
                            .Date("createdAt")
                        )
                    )
                )
            ), cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            _logger.LogError("Failed to create index '{Index}': {Reason}", ElasticsearchIndices.Tickets, createResponse.DebugInformation);
        }
        else
        {
            _logger.LogInformation("Successfully created index '{Index}'.", ElasticsearchIndices.Tickets);
        }
    }

    private async Task EnsureUsersIndexAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ElasticsearchIndices.Users, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        _logger.LogInformation("Creating Elasticsearch index '{Index}'...", ElasticsearchIndices.Users);

        var createResponse = await _client.Indices.CreateAsync(ElasticsearchIndices.Users, c => c
            .Mappings(m => m
                .Properties<UserDocument>(p => p
                    .Keyword(k => k.Id)
                    .Keyword(k => k.OrganizationId)
                    .Text(t => t.UserName, t => t.Fields(f => f.Keyword("keyword")))
                    .Keyword(k => k.Email)
                    .ShortNumber(s => s.Role)
                )
            ), cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            _logger.LogError("Failed to create index '{Index}': {Reason}", ElasticsearchIndices.Users, createResponse.DebugInformation);
        }
        else
        {
            _logger.LogInformation("Successfully created index '{Index}'.", ElasticsearchIndices.Users);
        }
    }

    private async Task EnsureOrganizationsIndexAsync(CancellationToken cancellationToken)
    {
        var existsResponse = await _client.Indices.ExistsAsync(ElasticsearchIndices.Organizations, cancellationToken);
        if (existsResponse.Exists)
        {
            return;
        }

        _logger.LogInformation("Creating Elasticsearch index '{Index}'...", ElasticsearchIndices.Organizations);

        var createResponse = await _client.Indices.CreateAsync(ElasticsearchIndices.Organizations, c => c
            .Mappings(m => m
                .Properties<OrganizationDocument>(p => p
                    .Keyword(k => k.Id)
                    .Text(t => t.Name, t => t.Fields(f => f.Keyword("keyword")))
                )
            ), cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            _logger.LogError("Failed to create index '{Index}': {Reason}", ElasticsearchIndices.Organizations, createResponse.DebugInformation);
        }
        else
        {
            _logger.LogInformation("Successfully created index '{Index}'.", ElasticsearchIndices.Organizations);
        }
    }
}
