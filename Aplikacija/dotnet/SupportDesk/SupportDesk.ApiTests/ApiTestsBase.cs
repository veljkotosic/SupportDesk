using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Respawn;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Infrastructure.Persistence.Database;
using Testcontainers.PostgreSql;

namespace SupportDesk.ApiTests;

internal abstract class ApiTestsBase
{
    protected const string DefaultCustomerEmail = "customer@test.com";
    protected const string DefaultCustomerPassword = "Test12345!";
    protected const string DefaultCustomerUserName = "TestCustomer";
    
    protected const string DefaultOrganizationName = "Test Organization";
    protected const string DefaultOrganizationAdminEmail = "orgadmin@test.com";
    protected const string DefaultOrganizationAdminPassword = "Test12345!";
    protected const string DefaultOrganizationAdminUserName = "TestOrgAdmin";

    protected const string DefaultSupportAgentEmail = "supportagent@test.com";
    protected const string DefaultSupportAgentPassword = "Test12345!";
    protected const string DefaultSupportAgentUserName = "TestSupportAgent";
    
    private static PostgreSqlContainer _postgresContainer = null!;
    private static DbConnection _dbConnection = null!;
    private static Respawner _respawner = null!;
    
    protected static WebApplicationFactory<Program> Factory = null!;
    protected HttpClient Client = null!;
    private IServiceScope? _scope;

    [OneTimeSetUp]
    public async Task GlobalOneTimeSetUp()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:latest")
            .WithName("SupportDeskApiTestPostgres")
            .WithDatabase("SupportDeskApiTestDatabase")
            .WithUsername("SupportDeskApiTestUser")
            .WithPassword("SupportDeskApiTestPassword")
            .Build();

        await _postgresContainer.StartAsync();

        Environment.SetEnvironmentVariable("DB_CONN_STRING", _postgresContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("RABBITMQ_AMQP_URI", "amqp://guest:guest@localhost:5672/");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "SupportDeskApiTest");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "SupportDeskApiTest");
        Environment.SetEnvironmentVariable("JWT_KEY", "SuperSecretApiTestKey12345678901234567890!");
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "60");
        Environment.SetEnvironmentVariable("JWT_CUSTOMER_REFRESH_TOKEN_EXPIRATION_DAYS", "5");
        Environment.SetEnvironmentVariable("JWT_ORGANIZATION_REFRESH_TOKEN_EXPIRATION_DAYS", "180");
        
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", "true");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4318");
        
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Warning); // Or LogLevel.None to silence completely
                });
            });

        using var migrationScope = Factory.Services.CreateScope();
        var dbContext = migrationScope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        await dbContext.Database.MigrateAsync();

        _dbConnection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    [OneTimeTearDown]
    public async Task GlobalOneTimeTearDown()
    {
        await _dbConnection.DisposeAsync();
        await Factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await _respawner.ResetAsync(_dbConnection);
        _scope = Factory.Services.CreateScope();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _scope?.Dispose();
        Client.Dispose();
    }

    protected IServiceScope Scope => _scope ?? throw new InvalidOperationException("Service scope has not been initialized.");
    protected IServiceProvider Services => Scope.ServiceProvider;

    protected T GetRequiredService<T>() where T : notnull => Services.GetRequiredService<T>();
    protected T? GetService<T>() => Services.GetService<T>();

    protected SupportDeskDbContext DbContext => GetRequiredService<SupportDeskDbContext>();
    protected IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();
    protected IPermissionService PermissionService => GetRequiredService<IPermissionService>();

    protected async Task AuthenticateAs(User user)
    {
        var tokenProvider = GetRequiredService<ITokenProvider>();
        var refreshTokenManager = GetRequiredService<IRefreshTokenManager>();
        
        var accessToken = tokenProvider.GenerateAccessToken(user);
        var refreshTokenValue = tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await refreshTokenManager.AddAsync(refreshTokenValue, user.Id.IdValue, user.Role, TimeProvider.System);
        await UnitOfWork.SaveChangesAsync();
        
        Client.DefaultRequestHeaders.Add("Cookie", $"accessToken={accessToken.Value}");
        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={refreshToken.Value}");
    }
    
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    protected async Task<User> SeedCustomerAsync(
        string email = DefaultCustomerEmail,
        string password = DefaultCustomerPassword,
        string userName = DefaultCustomerUserName)
    {
        var authService = GetRequiredService<IAuthService>();
        var user = User.Create(email, userName, null, UserRole.Customer, TimeProvider.System);
    
        await DbContext.Set<User>().AddAsync(user);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);
        await DbContext.SaveChangesAsync();
    
        DbContext.ChangeTracker.Clear();
        return user;
    }

    protected async Task<User> SeedOrganizationAdmin(
        string organizationName = DefaultOrganizationName,
        string email = DefaultOrganizationAdminEmail,
        string username = DefaultOrganizationAdminUserName,
        string password = DefaultOrganizationAdminPassword)
    {
        var authService = GetRequiredService<IAuthService>();
        var organization = Organization.Create(organizationName, TimeProvider.System);
    
        var user = User.Create(
            email,
            username,
            organization.Id.IdValue,
            UserRole.OrganizationAdmin,
            TimeProvider.System);
        
        await DbContext.Set<Organization>().AddAsync(organization);
        await DbContext.Set<User>().AddAsync(user);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);
        await DbContext.SaveChangesAsync();
    
        DbContext.ChangeTracker.Clear();
        return user;
    }

    protected async Task<User> SeedSupportAgent(
        Guid organizationId,
        string username = DefaultSupportAgentUserName,
        string email = DefaultSupportAgentEmail,
        string password = DefaultSupportAgentPassword)
    {
        var authService = GetRequiredService<IAuthService>();
    
        var user = User.Create(
            email,
            username,
            organizationId,
            UserRole.SupportAgent,
            TimeProvider.System);
    
        await DbContext.Set<User>().AddAsync(user);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);
        await DbContext.SaveChangesAsync();
    
        DbContext.ChangeTracker.Clear();
        return user;
    }

    protected async Task<SupportAgentInvite> CreateSupportAgentInvite(
        string email, 
        Guid organizationId, 
        TimeProvider? timeProvider = null)
    {
        var invite = await DbContext.Set<SupportAgentInvite>()
            .AddAsync(SupportAgentInvite.Create(email, organizationId, timeProvider ?? TimeProvider.System));
        
        await DbContext.SaveChangesAsync();
        
        return invite.Entity;
    }

    protected async Task<Category> CreateCategory(
        Guid organizationId,
        string name,
        string description,
        TimeProvider? timeProvider = null)
    {
        var category = await DbContext.Set<Category>()
            .AddAsync(Category.Create(organizationId, name, description, timeProvider ?? TimeProvider.System));
        
        await DbContext.SaveChangesAsync();
        
        return category.Entity;
    }
    
    protected async Task<Faq> CreateFaq(
        Guid organizationId,
        string question,
        string answer,
        TimeProvider? timeProvider = null)
    {
        var faq = await DbContext.Set<Faq>()
            .AddAsync(Faq.Create(organizationId, question, answer, timeProvider ?? TimeProvider.System));
        
        await DbContext.SaveChangesAsync();
        
        return faq.Entity;
    }
    
    protected async Task<TemplateAnswer> CreateTemplateAnswer(
        Guid organizationId,
        string title,
        string text,
        TimeProvider? timeProvider = null)
    {
        var templateAnswer = await DbContext.Set<TemplateAnswer>()
            .AddAsync(TemplateAnswer.Create(organizationId, title, text, timeProvider ?? TimeProvider.System));
        
        await DbContext.SaveChangesAsync();
        
        return templateAnswer.Entity;
    }
    
    protected async Task<Ticket> CreateTicket(Guid organizationId, Guid categoryId, Guid customerId, TicketPriority priority, string subject, string initialMessage, TimeProvider timeProvider)
    {
        var ticket = await DbContext.Set<Ticket>()
            .AddAsync(Ticket.Open(organizationId, customerId, categoryId, priority, subject, timeProvider));
        
        _ = await DbContext.Set<Message>()
            .AddAsync(Message.Create(organizationId, ticket.Entity.Id.IdValue, customerId, initialMessage, timeProvider));
        
        await DbContext.SaveChangesAsync();
        
        return ticket.Entity;   
    }
}
