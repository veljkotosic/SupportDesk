using System.Diagnostics;
using System.Text.Json;
using Dapper;
using DotNetEnv;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Application.ReadModels;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Infrastructure.Elasticsearch.Initialization;

Env.TraversePath().NoClobber().Load();

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddLogging(b => b.AddConsole());

services.AddSupportDeskPersistence(configuration);
services.AddSupportDeskElasticsearch(configuration);

var sp = services.BuildServiceProvider();
var client = sp.GetRequiredService<ElasticsearchClient>();
var initializer = sp.GetRequiredService<ElasticsearchIndexInitializer>();
var dbConnectionFactory = sp.GetRequiredService<IDbConnectionFactory>();

Console.WriteLine("==========================================================");
Console.WriteLine("   SUPPORTDESK ELASTICSEARCH SYNC & DUMP GENERATOR        ");
Console.WriteLine("==========================================================\n");

// 1. Locate k6-tests/data directory
var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
DirectoryInfo? dataDir = null;
while (currentDir != null)
{
    var candidate = Path.Combine(currentDir.FullName, "Aplikacija", "k6-tests", "data");
    if (Directory.Exists(candidate))
    {
        dataDir = new DirectoryInfo(candidate);
        break;
    }
    candidate = Path.Combine(currentDir.FullName, "k6-tests", "data");
    if (Directory.Exists(candidate))
    {
        dataDir = new DirectoryInfo(candidate);
        break;
    }
    currentDir = currentDir.Parent;
}

if (dataDir == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[!] Could not locate 'k6-tests/data' directory.");
    Console.ResetColor();
    return;
}

var dumpFilePath = Path.Combine(dataDir.FullName, "seed-es.ndjson");
Console.WriteLine($"[+] Target ndjson dump file: {dumpFilePath}");

// 2. Reset and initialize Elasticsearch indices
Console.WriteLine("\n[+] Recreating Elasticsearch indices...");
await client.Indices.DeleteAsync(ElasticsearchIndices.Tickets);
await client.Indices.DeleteAsync(ElasticsearchIndices.Users);
await client.Indices.DeleteAsync(ElasticsearchIndices.Organizations);

await initializer.EnsureIndicesCreatedAsync();
Console.WriteLine("[✔] Indices created with production mappings.");

var stopwatch = Stopwatch.StartNew();
await using var fileStream = new FileStream(dumpFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536);
await using var writer = new StreamWriter(fileStream, System.Text.Encoding.UTF8);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

await using var connection = await dbConnectionFactory.CreateOpenConnectionAsync();

// 3. Export Organizations
Console.WriteLine("\n[+] Processing Organizations...");
const string orgsSql = "SELECT \"Id\", \"Name\" FROM \"Organizations\";";
var orgs = (await connection.QueryAsync<OrganizationDocument>(orgsSql)).ToList();

if (orgs.Count > 0)
{
    foreach (var org in orgs)
    {
        var actionLine = $"{{\"index\":{{\"_index\":\"{ElasticsearchIndices.Organizations}\",\"_id\":\"{org.Id}\"}}}}";
        var docLine = JsonSerializer.Serialize(org, jsonOptions);
        await writer.WriteLineAsync(actionLine);
        await writer.WriteLineAsync(docLine);
    }

    var bulkOrgs = await client.BulkAsync(b => b
        .Index(ElasticsearchIndices.Organizations)
        .IndexMany(orgs, (descriptor, o) => descriptor.Id(o.Id.ToString())));

    if (!bulkOrgs.IsValidResponse)
    {
        Console.WriteLine($"[!] Error indexing organizations: {bulkOrgs.DebugInformation}");
    }
    else
    {
        Console.WriteLine($"[✔] Synchronized {orgs.Count:N0} organizations.");
    }
}

// 4. Export Users
Console.WriteLine("\n[+] Processing Users...");
const string usersSql = "SELECT \"Id\", \"OrganizationId\", \"UserName\", \"Email\", \"Role\" FROM \"Users\";";
await using var userCmd = connection.CreateCommand();
userCmd.CommandText = usersSql;
await using var userReader = await userCmd.ExecuteReaderAsync();

const int batchSize = 5000;
var userBatch = new List<UserDocument>(batchSize);
var totalUsers = 0;

while (await userReader.ReadAsync())
{
    var user = new UserDocument
    {
        Id = userReader.GetGuid(0),
        OrganizationId = userReader.IsDBNull(1) ? null : userReader.GetGuid(1),
        UserName = userReader.GetString(2),
        Email = userReader.GetString(3),
        Role = userReader.GetInt16(4)
    };

    var actionLine = $"{{\"index\":{{\"_index\":\"{ElasticsearchIndices.Users}\",\"_id\":\"{user.Id}\"}}}}";
    var docLine = JsonSerializer.Serialize(user, jsonOptions);
    await writer.WriteLineAsync(actionLine);
    await writer.WriteLineAsync(docLine);

    userBatch.Add(user);

    if (userBatch.Count >= batchSize)
    {
        await client.BulkAsync(b => b
            .Index(ElasticsearchIndices.Users)
            .IndexMany(userBatch, (d, u) => d.Id(u.Id.ToString())));
        totalUsers += userBatch.Count;
        userBatch.Clear();
    }
}

if (userBatch.Count > 0)
{
    await client.BulkAsync(b => b
        .Index(ElasticsearchIndices.Users)
        .IndexMany(userBatch, (d, u) => d.Id(u.Id.ToString())));
    totalUsers += userBatch.Count;
    userBatch.Clear();
}
Console.WriteLine($"[✔] Synchronized {totalUsers:N0} users.");
await userReader.CloseAsync();

// 5. Export Tickets
Console.WriteLine("\n[+] Processing Tickets and Unread Notifications...");
const string notifSql = """
    SELECT "Id", "OrganizationId", "TicketId", "Text", "Status", "CreatedAt"
    FROM "TicketNotifications"
    WHERE "Status" = 0;
    """;
var notifs = (await connection.QueryAsync<TicketNotificationDocument>(notifSql)).ToList();
var notifsByTicket = notifs.GroupBy(n => n.TicketId).ToDictionary(g => g.Key, g => g.ToList());

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

await using var ticketCmd = connection.CreateCommand();
ticketCmd.CommandText = ticketsSql;
ticketCmd.CommandTimeout = 600;
await using var ticketReader = await ticketCmd.ExecuteReaderAsync();

var ticketBatch = new List<TicketDocument>(batchSize);
var totalTickets = 0;

while (await ticketReader.ReadAsync())
{
    var ticket = new TicketDocument
    {
        Id = ticketReader.GetGuid(0),
        OrganizationId = ticketReader.GetGuid(1),
        OrganizationName = ticketReader.GetString(2),
        CategoryId = ticketReader.GetGuid(3),
        CategoryName = ticketReader.GetString(4),
        CustomerId = ticketReader.GetGuid(5),
        CustomerUserName = ticketReader.GetString(6),
        CustomerEmail = ticketReader.GetString(7),
        SupportAgentId = ticketReader.IsDBNull(8) ? null : ticketReader.GetGuid(8),
        SupportAgentUserName = ticketReader.IsDBNull(9) ? null : ticketReader.GetString(9),
        Status = ticketReader.GetInt16(10),
        Priority = ticketReader.GetInt16(11),
        Feedback = ticketReader.IsDBNull(12) ? (short)0 : ticketReader.GetInt16(12),
        Subject = ticketReader.GetString(13),
        OpenedAt = ticketReader.GetDateTime(14),
        AssignedAt = ticketReader.IsDBNull(15) ? null : ticketReader.GetDateTime(15),
        ClosedAt = ticketReader.IsDBNull(16) ? null : ticketReader.GetDateTime(16),
        LastMessageAt = ticketReader.IsDBNull(17) ? null : ticketReader.GetDateTime(17),
        UnreadNotifications = []
    };

    if (notifsByTicket.TryGetValue(ticket.Id, out var unreadList))
    {
        ticket.UnreadNotifications = unreadList;
    }

    var actionLine = $"{{\"index\":{{\"_index\":\"{ElasticsearchIndices.Tickets}\",\"_id\":\"{ticket.Id}\"}}}}";
    var docLine = JsonSerializer.Serialize(ticket, jsonOptions);
    await writer.WriteLineAsync(actionLine);
    await writer.WriteLineAsync(docLine);

    ticketBatch.Add(ticket);

    if (ticketBatch.Count >= batchSize)
    {
        await client.BulkAsync(b => b
            .Index(ElasticsearchIndices.Tickets)
            .IndexMany(ticketBatch, (d, t) => d.Id(t.Id.ToString())));
        totalTickets += ticketBatch.Count;
        if (totalTickets % 25000 == 0)
        {
            Console.WriteLine($"    -> Progress: {totalTickets:N0} tickets indexed and written to dump...");
        }
        ticketBatch.Clear();
    }
}

if (ticketBatch.Count > 0)
{
    await client.BulkAsync(b => b
        .Index(ElasticsearchIndices.Tickets)
        .IndexMany(ticketBatch, (d, t) => d.Id(t.Id.ToString())));
    totalTickets += ticketBatch.Count;
    ticketBatch.Clear();
}

await ticketReader.CloseAsync();
await writer.FlushAsync();
await fileStream.FlushAsync();

stopwatch.Stop();

Console.WriteLine($"\n[✔] Finished in {stopwatch.Elapsed.TotalSeconds:F1}s!");
Console.WriteLine($"[✔] Dump file generated: {dumpFilePath} ({new FileInfo(dumpFilePath).Length / (1024 * 1024):N0} MB)");

// 6. Refresh and verify counts in Elasticsearch
await client.Indices.RefreshAsync(ElasticsearchIndices.Organizations);
await client.Indices.RefreshAsync(ElasticsearchIndices.Users);
await client.Indices.RefreshAsync(ElasticsearchIndices.Tickets);

var orgCount = await client.CountAsync(ElasticsearchIndices.Organizations);
var userCount = await client.CountAsync(ElasticsearchIndices.Users);
var ticketCount = await client.CountAsync(ElasticsearchIndices.Tickets);

Console.WriteLine("\n[+] Verification in Elasticsearch:");
Console.WriteLine($"    Organizations : {orgCount.Count:N0}");
Console.WriteLine($"    Users         : {userCount.Count:N0}");
Console.WriteLine($"    Tickets       : {ticketCount.Count:N0}");
Console.WriteLine("\n[✔] ALL READ MODELS SYNCHRONIZED SUCCESSFULLY!");
