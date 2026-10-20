using Bogus;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Options;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Message.Options;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Note.Options;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.Options;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Options;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.Ticket.Options;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Infrastructure.Auth.Identity;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Infrastructure.Persistence.Database;

Env.TraversePath().NoClobber().Load();

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

services.AddSingleton<IUserContext, SystemUserContext>();
services.AddSingleton<ITenantContext, SystemTenantContext>();

services.AddSupportDeskPersistence(configuration);
services.AddSupportDeskAuth(configuration);

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppIdentityUser>>();
var timeProvider = TimeProvider.System;

dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

Console.WriteLine("==============================================");
Console.WriteLine("      SUPPORTDESK DATABASE SEED GENERATOR     ");
Console.WriteLine("==============================================\n");
Console.WriteLine(" Tip: For count inputs, you can enter a single number (e.g. 5) or a range (e.g. 0-3).\n");

int orgCount = PromptCount("1. Number of Organizations (and Admins): ", minAllowed: 1);
var agentsRange = PromptRange("2. Support Agents per Organization (e.g. 2-5 or 3): ", minAllowed: 1);
int customersCount = PromptCount("3. Number of Total Customers (e.g. 100000): ", minAllowed: 1);
var categoriesRange = PromptRange("4. Categories per Organization (e.g. 3-6 or 4): ", minAllowed: 1);
var faqsRange = PromptRange("5. FAQs per Organization (e.g. 2-8 or 5): ", minAllowed: 0);
var templateAnswersRange = PromptRange("6. Template Answers per Organization (e.g. 2-6 or 4): ", minAllowed: 0);
var ticketsRange = PromptRange("7. Tickets per Customer (e.g. 0-2 or 1): ", minAllowed: 0);
var messagesRange = PromptRange("8. Messages per Ticket (e.g. 1-4 or 2): ", minAllowed: 1);
var notesRange = PromptRange("9. Notes per Ticket (e.g. 0-2 or 1): ", minAllowed: 0);

Console.WriteLine("\n[+] Resetting and applying migrations...");
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.MigrateAsync();

var stopwatch = System.Diagnostics.Stopwatch.StartNew();
var faker = new Faker();
var random = new Random(42);
const string defaultPassword = "Password123!";

var sampleIdentity = new AppIdentityUser();
var precomputedPasswordHash = passwordHasher.HashPassword(sampleIdentity, defaultPassword);

Console.WriteLine("[+] Seeding Organizations & Admins...");
var organizations = new List<Organization>(orgCount);
var orgAdmins = new List<User>(orgCount);
var identityUsers = new List<AppIdentityUser>(orgCount);

for (int i = 1; i <= orgCount; i++)
{
    var rawOrgName = $"{faker.Company.CompanyName().Replace(",", "").Replace(".", "").Replace("'", "")} {i}";
    var orgName = Truncate(rawOrgName, OrganizationOptionsDefaults.NameMaximumLength);
    if (orgName.Length < OrganizationOptionsDefaults.NameMinimumLength)
    {
        orgName = $"Org_{i:D6}";
    }

    var org = Organization.Create(orgName, timeProvider);
    org.ClearDomainEvents();
    organizations.Add(org);

    var adminEmail = $"admin{i}@org{i}.com";
    var adminUserName = $"admin_{i:D6}";
    var domainAdmin = User.Create(
        adminEmail,
        adminUserName,
        org.Id.IdValue,
        UserRole.OrganizationAdmin,
        timeProvider);
    domainAdmin.ClearDomainEvents();
    orgAdmins.Add(domainAdmin);

    identityUsers.Add(new AppIdentityUser
    {
        Id = domainAdmin.Id.IdValue,
        UserName = adminUserName,
        NormalizedUserName = adminUserName.ToUpperInvariant(),
        Email = adminEmail,
        NormalizedEmail = adminEmail.ToUpperInvariant(),
        EmailConfirmed = true,
        PasswordHash = precomputedPasswordHash,
        SecurityStamp = Guid.NewGuid().ToString()
    });
}

await dbContext.Set<Organization>().AddRangeAsync(organizations);
await dbContext.Set<User>().AddRangeAsync(orgAdmins);
await dbContext.Set<AppIdentityUser>().AddRangeAsync(identityUsers);
await dbContext.SaveChangesAsync();
identityUsers.Clear();

Console.WriteLine("[+] Seeding Support Agents...");
var supportAgents = new List<User>();
var supportAgentsByOrg = new Dictionary<Guid, List<User>>();
int agentIndex = 1;

foreach (var org in organizations)
{
    var orgAgents = new List<User>();
    int agentCount = random.Next(agentsRange.Min, agentsRange.Max + 1);

    for (int j = 1; j <= agentCount; j++)
    {
        var agentEmail = $"agent{agentIndex}@org{org.Id.IdValue.ToString()[..4]}.com";
        var agentUserName = $"agent_{agentIndex:D6}";

        var agent = User.Create(
            agentEmail,
            agentUserName,
            org.Id.IdValue,
            UserRole.SupportAgent,
            timeProvider);
        agent.ClearDomainEvents();
        orgAgents.Add(agent);
        supportAgents.Add(agent);

        identityUsers.Add(new AppIdentityUser
        {
            Id = agent.Id.IdValue,
            UserName = agentUserName,
            NormalizedUserName = agentUserName.ToUpperInvariant(),
            Email = agentEmail,
            NormalizedEmail = agentEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = precomputedPasswordHash,
            SecurityStamp = Guid.NewGuid().ToString()
        });

        agentIndex++;
    }

    supportAgentsByOrg[org.Id.IdValue] = orgAgents;
}

await dbContext.Set<User>().AddRangeAsync(supportAgents);
await dbContext.Set<AppIdentityUser>().AddRangeAsync(identityUsers);
await dbContext.SaveChangesAsync();
identityUsers.Clear();

Console.WriteLine("[+] Seeding Categories, FAQs & Template Answers...");
var categoriesByOrg = new Dictionary<Guid, List<Category>>();
var allCategories = new List<Category>();
var allFaqs = new List<Faq>();
var allTemplateAnswers = new List<TemplateAnswer>();

foreach (var org in organizations)
{
    int catCount = random.Next(categoriesRange.Min, categoriesRange.Max + 1);
    var orgCategories = new List<Category>(catCount);

    for (int k = 1; k <= catCount; k++)
    {
        var categoryName = $"Category-{k}";
        var categoryDesc = Truncate(faker.Lorem.Sentence(5), CategoryOptionsDefaults.DescriptionMaximumLength);
        var category = Category.Create(org.Id.IdValue, categoryName, categoryDesc, timeProvider);
        category.ClearDomainEvents();

        orgCategories.Add(category);
        allCategories.Add(category);
    }
    categoriesByOrg[org.Id.IdValue] = orgCategories;

    int faqCount = random.Next(faqsRange.Min, faqsRange.Max + 1);
    for (int f = 1; f <= faqCount; f++)
    {
        var question = Truncate(faker.Lorem.Sentence(4) + "?", FaqOptionsDefaults.QuestionMaximumLength);
        var answer = Truncate(faker.Lorem.Paragraph(), FaqOptionsDefaults.AnswerMaximumLength);
        var faq = Faq.Create(org.Id.IdValue, question, answer, timeProvider);
        faq.ClearDomainEvents();
        allFaqs.Add(faq);
    }

    int tplCount = random.Next(templateAnswersRange.Min, templateAnswersRange.Max + 1);
    for (int t = 1; t <= tplCount; t++)
    {
        var title = Truncate(faker.Lorem.Sentence(3), TemplateAnswerOptionsDefaults.TitleMaximumLength);
        var text = Truncate(faker.Lorem.Paragraph(), TemplateAnswerOptionsDefaults.TextMaximumLength);
        var templateAnswer = TemplateAnswer.Create(org.Id.IdValue, title, text, timeProvider);
        templateAnswer.ClearDomainEvents();
        allTemplateAnswers.Add(templateAnswer);
    }
}

await dbContext.Set<Category>().AddRangeAsync(allCategories);
if (allFaqs.Count > 0) await dbContext.Set<Faq>().AddRangeAsync(allFaqs);
if (allTemplateAnswers.Count > 0) await dbContext.Set<TemplateAnswer>().AddRangeAsync(allTemplateAnswers);
await dbContext.SaveChangesAsync();

allCategories.Clear();
allFaqs.Clear();
allTemplateAnswers.Clear();
dbContext.ChangeTracker.Clear();

Console.WriteLine($"[+] Streaming {customersCount:N0} Customers and related Tickets in batches...");
var priorities = Enum.GetValues<TicketPriority>();

const int customerBatchSize = 5000;
int totalCustomersCreated = 0;
int totalTickets = 0;
int totalMessages = 0;
int totalNotes = 0;

var customerBatch = new List<User>(customerBatchSize);
var identityBatch = new List<AppIdentityUser>(customerBatchSize);
var ticketBatch = new List<Ticket>(customerBatchSize * 2);
var messageBatch = new List<Message>(customerBatchSize * 6);
var noteBatch = new List<Note>(customerBatchSize * 2);

for (int i = 1; i <= customersCount; i++)
{
    var customerEmail = $"customer{i}@test.com";
    var customerUserName = $"customer_{i:D7}";

    var customer = User.Create(
        customerEmail,
        customerUserName,
        null,
        UserRole.Customer,
        timeProvider);
    customer.ClearDomainEvents();
    customerBatch.Add(customer);

    identityBatch.Add(new AppIdentityUser
    {
        Id = customer.Id.IdValue,
        UserName = customerUserName,
        NormalizedUserName = customerUserName.ToUpperInvariant(),
        Email = customerEmail,
        NormalizedEmail = customerEmail.ToUpperInvariant(),
        EmailConfirmed = true,
        PasswordHash = precomputedPasswordHash,
        SecurityStamp = Guid.NewGuid().ToString()
    });

    int ticketCount = random.Next(ticketsRange.Min, ticketsRange.Max + 1);

    for (int t = 0; t < ticketCount; t++)
    {
        var targetOrg = organizations[random.Next(organizations.Count)];
        var targetCategories = categoriesByOrg[targetOrg.Id.IdValue];
        var category = targetCategories[random.Next(targetCategories.Count)];
        var priority = priorities[random.Next(priorities.Length)];
        var subject = Truncate(faker.Lorem.Sentence(3), TicketOptionsDefaults.SubjectMaximumLength);

        var ticket = Ticket.Open(
            organizationId: targetOrg.Id.IdValue,
            customerId: customer.Id.IdValue,
            categoryId: category.Id.IdValue,
            priority: priority,
            subject: subject,
            timeProvider: timeProvider);

        var orgAgents = supportAgentsByOrg[targetOrg.Id.IdValue];
        var orgAdmin = orgAdmins.FirstOrDefault(a => a.OrganizationId?.IdValue == targetOrg.Id.IdValue);

        if (orgAgents.Count > 0 && random.NextDouble() > 0.3)
        {
            var assignedAgent = orgAgents[random.Next(orgAgents.Count)];
            ticket.Assign(assignedAgent.Id.IdValue, timeProvider);
        }

        ticket.ClearDomainEvents();
        ticketBatch.Add(ticket);
        totalTickets++;

        int messageCount = random.Next(messagesRange.Min, messagesRange.Max + 1);
        for (int m = 0; m < messageCount; m++)
        {
            bool isFromCustomer = m % 2 == 0;
            var senderId = isFromCustomer
                ? customer.Id.IdValue
                : (ticket.SupportAgentId?.IdValue ?? (orgAgents.Count > 0 ? orgAgents[0].Id.IdValue : customer.Id.IdValue));

            var messageText = Truncate(faker.Lorem.Sentence(8), MessageOptionsDefaults.TextMaximumLength);
            var message = Message.Create(
                organizationId: targetOrg.Id.IdValue,
                ticketId: ticket.Id.IdValue,
                senderId: senderId,
                text: messageText,
                timeProvider: timeProvider);

            message.ClearDomainEvents();
            messageBatch.Add(message);
            totalMessages++;
        }

        int noteCount = random.Next(notesRange.Min, notesRange.Max + 1);
        for (int n = 0; n < noteCount; n++)
        {
            var authorId = ticket.SupportAgentId?.IdValue 
                           ?? (orgAgents.Count > 0 ? orgAgents[random.Next(orgAgents.Count)].Id.IdValue : orgAdmin?.Id.IdValue ?? customer.Id.IdValue);

            var noteText = Truncate(faker.Lorem.Sentence(6), NoteOptionsDefaults.TextMaximumLength);
            var note = Note.Create(
                organizationId: targetOrg.Id.IdValue,
                ticketId: ticket.Id.IdValue,
                authorId: authorId,
                text: noteText,
                timeProvider: timeProvider);

            note.ClearDomainEvents();
            noteBatch.Add(note);
            totalNotes++;
        }
    }

    totalCustomersCreated++;

    if (customerBatch.Count >= customerBatchSize)
    {
        await dbContext.Set<User>().AddRangeAsync(customerBatch);
        await dbContext.Set<AppIdentityUser>().AddRangeAsync(identityBatch);
        if (ticketBatch.Count > 0) await dbContext.Set<Ticket>().AddRangeAsync(ticketBatch);
        if (messageBatch.Count > 0) await dbContext.Set<Message>().AddRangeAsync(messageBatch);
        if (noteBatch.Count > 0) await dbContext.Set<Note>().AddRangeAsync(noteBatch);

        await dbContext.SaveChangesAsync();

        customerBatch.Clear();
        identityBatch.Clear();
        ticketBatch.Clear();
        messageBatch.Clear();
        noteBatch.Clear();
        dbContext.ChangeTracker.Clear();

        Console.WriteLine($"    -> Progress: {totalCustomersCreated:N0}/{customersCount:N0} customers ({totalTickets:N0} tickets, {totalMessages:N0} messages)...");
    }
}

if (customerBatch.Count > 0)
{
    await dbContext.Set<User>().AddRangeAsync(customerBatch);
    await dbContext.Set<AppIdentityUser>().AddRangeAsync(identityBatch);
    if (ticketBatch.Count > 0) await dbContext.Set<Ticket>().AddRangeAsync(ticketBatch);
    if (messageBatch.Count > 0) await dbContext.Set<Message>().AddRangeAsync(messageBatch);
    if (noteBatch.Count > 0) await dbContext.Set<Note>().AddRangeAsync(noteBatch);

    await dbContext.SaveChangesAsync();

    customerBatch.Clear();
    identityBatch.Clear();
    ticketBatch.Clear();
    messageBatch.Clear();
    noteBatch.Clear();
    dbContext.ChangeTracker.Clear();
}

stopwatch.Stop();
Console.WriteLine($"\n[✔] Successfully seeded {totalCustomersCreated:N0} customers, {totalTickets:N0} tickets, {totalMessages:N0} messages, and {totalNotes:N0} notes in {stopwatch.Elapsed.TotalSeconds:F2}s!");

static string Truncate(string value, int maxLength)
{
    if (string.IsNullOrEmpty(value)) return value;
    return value.Length <= maxLength ? value : value[..maxLength];
}

static int PromptCount(string message, int minAllowed)
{
    while (true)
    {
        Console.Write(message);
        var input = Console.ReadLine();
        if (int.TryParse(input, out int result) && result >= minAllowed)
        {
            return result;
        }
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  --> Please enter a valid integer ({minAllowed} or greater).");
        Console.ResetColor();
    }
}

static (int Min, int Max) PromptRange(string message, int minAllowed)
{
    while (true)
    {
        Console.Write(message);
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        if (input.Contains('-'))
        {
            var parts = input.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out int min) &&
                int.TryParse(parts[1].Trim(), out int max) &&
                min >= minAllowed &&
                max >= min)
            {
                return (min, max);
            }
        }
        else if (int.TryParse(input, out int singleVal) && singleVal >= minAllowed)
        {
            return (singleVal, singleVal);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  --> Please enter a valid count or range (e.g. '{minAllowed}-3' or '2', with minimum value >= {minAllowed}).");
        Console.ResetColor();
    }
}

public class SystemUserContext : IUserContext
{
    public Guid? TryGetCurrentUserId() => null;
    public Guid GetCurrentUserId() => Guid.Empty;
}

public class SystemTenantContext : ITenantContext
{
    public Guid? GetCurrentOrganizationId() => null;
}