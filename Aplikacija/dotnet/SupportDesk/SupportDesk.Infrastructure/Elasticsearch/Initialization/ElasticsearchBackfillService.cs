using Dapper;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.ReadModels;

namespace SupportDesk.Infrastructure.Elasticsearch.Initialization;

public sealed class ElasticsearchBackfillService : BackgroundService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchIndexInitializer _initializer;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<ElasticsearchBackfillService> _logger;

    public ElasticsearchBackfillService(
        ElasticsearchClient client,
        ElasticsearchIndexInitializer initializer,
        IDbConnectionFactory dbConnectionFactory,
        ILogger<ElasticsearchBackfillService> logger)
    {
        _client = client;
        _initializer = initializer;
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunBackfillAsync(forceReindex: false, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Worker is stopping
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete Elasticsearch backfill.");
        }
    }

    public async Task RunBackfillAsync(bool forceReindex = false, CancellationToken cancellationToken = default)
    {
        if (forceReindex)
        {
            _logger.LogInformation("Force reindex requested. Deleting existing Elasticsearch indices...");
            
            await _client.Indices.DeleteAsync(ElasticsearchIndices.Tickets, cancellationToken);
            await _client.Indices.DeleteAsync(ElasticsearchIndices.Users, cancellationToken);
            await _client.Indices.DeleteAsync(ElasticsearchIndices.Organizations, cancellationToken);
        }

        await _initializer.EnsureIndicesCreatedAsync(cancellationToken);
        
        await BackfillOrganizationsAsync(forceReindex, cancellationToken);
        await BackfillUsersAsync(forceReindex, cancellationToken);
        await BackfillTicketsAsync(forceReindex, cancellationToken);
    }

    private async Task BackfillOrganizationsAsync(bool forceReindex, CancellationToken cancellationToken)
    {
        if (!forceReindex)
        {
            var count = await _client.CountAsync(ElasticsearchIndices.Organizations, cancellationToken);
            if (count.Count > 0)
            {
                return;
            }
        }

        _logger.LogInformation("Backfilling organizations into Elasticsearch...");

        const string sql = "SELECT \"Id\", \"Name\" FROM \"Organizations\";";
        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var orgs = (await connection.QueryAsync<OrganizationDocument>(sql)).ToList();

        if (orgs.Count == 0) return;

        var bulkResponse = await _client.BulkAsync(b => b
            .Index(ElasticsearchIndices.Organizations)
            .IndexMany(orgs, (descriptor, org) => descriptor.Id(org.Id.ToString())), cancellationToken);

        if (!bulkResponse.IsValidResponse)
        {
            _logger.LogError("Failed bulk indexing organizations: {Reason}", bulkResponse.DebugInformation);
        }
        else
        {
            _logger.LogInformation("Successfully backfilled {Count} organizations into Elasticsearch.", orgs.Count);
        }
    }

    private async Task BackfillUsersAsync(bool forceReindex, CancellationToken cancellationToken)
    {
        if (!forceReindex)
        {
            var count = await _client.CountAsync(ElasticsearchIndices.Users, cancellationToken);
            if (count.Count > 0)
            {
                return;
            }
        }

        _logger.LogInformation("Backfilling users into Elasticsearch...");

        const string sql = "SELECT \"Id\", \"OrganizationId\", \"UserName\", \"Email\", \"Role\" FROM \"Users\";";
        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        const int batchSize = 5000;
        var batch = new List<UserDocument>(batchSize);
        var totalIndexed = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            var user = new UserDocument
            {
                Id = reader.GetGuid(0),
                OrganizationId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                UserName = reader.GetString(2),
                Email = reader.GetString(3),
                Role = reader.GetInt16(4)
            };
            batch.Add(user);

            if (batch.Count >= batchSize)
            {
                await IndexUsersBatchAsync(batch, cancellationToken);
                totalIndexed += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await IndexUsersBatchAsync(batch, cancellationToken);
            totalIndexed += batch.Count;
            batch.Clear();
        }

        _logger.LogInformation("Successfully backfilled {Count} users into Elasticsearch.", totalIndexed);
    }

    private async Task IndexUsersBatchAsync(List<UserDocument> batch, CancellationToken cancellationToken)
    {
        var bulkResponse = await _client.BulkAsync(b => b
            .Index(ElasticsearchIndices.Users)
            .IndexMany(batch, (descriptor, user) => descriptor.Id(user.Id.ToString())), cancellationToken);

        if (!bulkResponse.IsValidResponse)
        {
            _logger.LogError("Failed bulk indexing users batch: {Reason}", bulkResponse.DebugInformation);
        }
    }

    private async Task BackfillTicketsAsync(bool forceReindex, CancellationToken cancellationToken)
    {
        if (!forceReindex)
        {
            var count = await _client.CountAsync(ElasticsearchIndices.Tickets, cancellationToken);
            if (count.Count > 0)
            {
                _logger.LogInformation("Elasticsearch index '{Index}' already contains {Count} documents. Skipping ticket backfill.", ElasticsearchIndices.Tickets, count.Count);
                return;
            }
        }

        _logger.LogInformation("Backfilling tickets from PostgreSQL to Elasticsearch...");

        await using var connection = await _dbConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        // 1. Load all unread notifications
        const string notificationsSql = """
            SELECT "Id", "OrganizationId", "TicketId", "Text", "Status", "CreatedAt"
            FROM "TicketNotifications"
            WHERE "Status" = 0;
            """;
        var notifications = (await connection.QueryAsync<TicketNotificationDocument>(notificationsSql)).ToList();
        var notificationsByTicket = notifications
            .GroupBy(n => n.TicketId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 2. Stream tickets in streaming batches of 5000
        const string ticketsSql = """
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
            LEFT JOIN "Users" sa ON sa."Id" = t."SupportAgentId";
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = ticketsSql;
        command.CommandTimeout = 600;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        const int batchSize = 5000;
        var batch = new List<TicketDocument>(batchSize);
        var totalIndexed = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            var ticket = new TicketDocument
            {
                Id = reader.GetGuid(0),
                OrganizationId = reader.GetGuid(1),
                OrganizationName = reader.GetString(2),
                CategoryId = reader.GetGuid(3),
                CategoryName = reader.GetString(4),
                CustomerId = reader.GetGuid(5),
                CustomerUserName = reader.GetString(6),
                CustomerEmail = reader.GetString(7),
                SupportAgentId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                SupportAgentUserName = reader.IsDBNull(9) ? null : reader.GetString(9),
                Status = reader.GetInt16(10),
                Priority = reader.GetInt16(11),
                Feedback = reader.IsDBNull(12) ? (short)0 : reader.GetInt16(12),
                Subject = reader.GetString(13),
                OpenedAt = reader.GetDateTime(14),
                AssignedAt = reader.IsDBNull(15) ? null : reader.GetDateTime(15),
                ClosedAt = reader.IsDBNull(16) ? null : reader.GetDateTime(16),
                LastMessageAt = reader.IsDBNull(17) ? null : reader.GetDateTime(17),
                UnreadNotifications = []
            };

            if (notificationsByTicket.TryGetValue(ticket.Id, out var unreadList))
            {
                ticket.UnreadNotifications = unreadList;
            }

            batch.Add(ticket);

            if (batch.Count >= batchSize)
            {
                await IndexTicketsBatchAsync(batch, cancellationToken);
                totalIndexed += batch.Count;
                if (totalIndexed % 25000 == 0)
                {
                    _logger.LogInformation("Indexed {Count} tickets into Elasticsearch...", totalIndexed);
                }
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await IndexTicketsBatchAsync(batch, cancellationToken);
            totalIndexed += batch.Count;
            batch.Clear();
        }

        _logger.LogInformation("Successfully backfilled total of {Count} tickets into Elasticsearch.", totalIndexed);
    }

    private async Task IndexTicketsBatchAsync(List<TicketDocument> batch, CancellationToken cancellationToken)
    {
        var bulkResponse = await _client.BulkAsync(b => b
            .Index(ElasticsearchIndices.Tickets)
            .IndexMany(batch, (descriptor, ticket) => descriptor.Id(ticket.Id.ToString())), cancellationToken);

        if (!bulkResponse.IsValidResponse)
        {
            _logger.LogError("Error during bulk indexing tickets batch: {Reason}", bulkResponse.DebugInformation);
        }
    }
}
