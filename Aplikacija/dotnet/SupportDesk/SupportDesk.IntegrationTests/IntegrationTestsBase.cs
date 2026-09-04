using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Respawn;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Infrastructure.Persistence.Database;
using Testcontainers.PostgreSql;

namespace SupportDesk.IntegrationTests;

internal abstract class IntegrationTestsBase
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
    
    private static PostgreSqlContainer _postgresSqlContainer = null!;
    private static DbConnection _dbConnection = null!;
    private static Respawner _respawner = null!;
    
    private static ServiceProvider _serviceProvider = null!;
    private static IConfiguration _configuration = null!;

    private IServiceScope? _scope;

    [OneTimeSetUp]
    public async Task GlobalOneTimeSetUp()
    {
        _postgresSqlContainer = new PostgreSqlBuilder("postgres:latest")
            .WithName("SupportDeskPostgresIntegrationTestsContainer")
            .WithDatabase("SupportDeskTestDatabase")
            .WithUsername("SupportDeskTestUser")
            .WithPassword("SupportDeskTestPassword")
            .Build();

        await _postgresSqlContainer.StartAsync();

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["DB_CONN_STRING"] = _postgresSqlContainer.GetConnectionString(),
            ["RABBITMQ_AMQP_URI"] = "amqp://guest:guest@localhost:5672/",
            ["JWT_ISSUER"] = "SupportDeskIntegrationTest",
            ["JWT_AUDIENCE"] = "SupportDeskIntegrationTest",
            ["JWT_KEY"] = "SuperSecretIntegrationTestKey12345678901234567890!",
            ["JWT_EXPIRATION_MINUTES"] = "60",
            ["JWT_CUSTOMER_REFRESH_TOKEN_EXPIRATION_DAYS"] = "5",
            ["JWT_ORGANIZATION_REFRESH_TOKEN_EXPIRATION_DAYS"] = "180"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        
        services.AddSupportDeskWebApi(_configuration);
        
        InterceptServices(services);

        _serviceProvider = services.BuildServiceProvider();
        using var migrationScope = _serviceProvider.CreateScope();
        
        var dbContext = migrationScope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
        
        await dbContext.Database.MigrateAsync();
        
        _dbConnection = new NpgsqlConnection(_postgresSqlContainer.GetConnectionString());
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
        await _serviceProvider.DisposeAsync();
        await _dbConnection.DisposeAsync();
        await _postgresSqlContainer.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();
        
        _scope = _serviceProvider.CreateScope();
    }

    [TearDown]
    public void TearDown()
    {
        _scope?.Dispose();
        _scope = null;
    }

    protected IServiceScope Scope => _scope ?? throw new InvalidOperationException("Service scope has not been initialized.");
    protected IServiceProvider Services => Scope.ServiceProvider;

    protected T GetRequiredService<T>() where T : notnull => Services.GetRequiredService<T>();
    protected T? GetService<T>() => Services.GetService<T>();
    
    protected SupportDeskDbContext DbContext => GetRequiredService<SupportDeskDbContext>();
    protected IUnitOfWork UnitOfWork => GetRequiredService<IUnitOfWork>();
    
    protected IPermissionService PermissionService => GetRequiredService<IPermissionService>();
    
    protected ICommandDispatcher CommandDispatcher => GetRequiredService<ICommandDispatcher>();
    protected IQueryDispatcher QueryDispatcher => GetRequiredService<IQueryDispatcher>();
    
    protected readonly Mock<IUserContext> UserContextMock = new();
    protected readonly Mock<ITenantContext> TenantContextMock = new();
    
    protected virtual void InterceptServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Scoped<IUserContext>(_ => UserContextMock.Object));
        services.Replace(ServiceDescriptor.Scoped<ITenantContext>(_ => TenantContextMock.Object));
    }
    
    protected async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    protected async Task<User> RegisterCustomer(
        string email = DefaultCustomerEmail, 
        string userName = DefaultCustomerUserName,
        string password = DefaultCustomerPassword)
    {
        var authService = GetRequiredService<IAuthService>();
        var userRepository = GetRequiredService<IUserRepository>();

        var user = User.Create(
            email,
            userName,
            null,
            UserRole.Customer,
            TimeProvider.System);
        
        await userRepository.SaveAsync(user);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);

        await UnitOfWork.SaveChangesAsync();
        
        return user;
    }

    protected async Task<User> RegisterOrganizationAdmin(
        string organizationName = DefaultOrganizationName,
        string email = DefaultOrganizationAdminEmail,
        string username = DefaultOrganizationAdminUserName,
        string password = DefaultOrganizationAdminPassword)
    {
        var authService = GetRequiredService<IAuthService>();
        var userRepository = GetRequiredService<IUserRepository>();
        var organizationRepository = GetRequiredService<IOrganizationRepository>();
        
        var organization = Organization.Create(organizationName, TimeProvider.System);
        
        var user = User.Create(
            email,
            username,
            organization.Id.IdValue,
            UserRole.OrganizationAdmin,
            TimeProvider.System);
        
        await userRepository.SaveAsync(user);
        await organizationRepository.SaveAsync(organization);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);
        
        await UnitOfWork.SaveChangesAsync();   
        
        return user;
    }

    protected async Task<User> RegisterSupportAgent(
        Guid organizationId,
        string email = DefaultSupportAgentEmail,
        string username = DefaultSupportAgentUserName,
        string password = DefaultSupportAgentPassword)
    {
        var authService = GetRequiredService<IAuthService>();
        var userRepository = GetRequiredService<IUserRepository>();

        var user = User.Create(
            email,
            username,
            organizationId,
            UserRole.SupportAgent,
            TimeProvider.System);
        
        await userRepository.SaveAsync(user);
        await authService.SignUpWithEmailAndPasswordAsync(user, password);
        
        await UnitOfWork.SaveChangesAsync();
        
        return user;   
    }

    protected async Task<RefreshToken> Login(string email, string password)
    {
        var authService = GetRequiredService<IAuthService>();
        var tokenProvider = GetRequiredService<ITokenProvider>();
        var refreshTokenManager = GetRequiredService<IRefreshTokenManager>();
        
        var user = await authService.LoginWithEmailAndPasswordAsync(email, password);
        
        var refreshTokenValue = tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await refreshTokenManager.AddAsync(refreshTokenValue, user.Id.IdValue, user.Role, TimeProvider.System);
        
        await UnitOfWork.SaveChangesAsync();
        
        return refreshToken;  
    }
    
    protected async Task<Category> CreateCategory(Guid organizationId, string name, string description, TimeProvider timeProvider)
    {
        var category = await DbContext.Set<Category>()
            .AddAsync(Category.Create(organizationId, name, description, timeProvider));
        
        await DbContext.SaveChangesAsync();
        
        return category.Entity;
    }

    protected async Task<Faq> CreateFaq(Guid organizationId, string question, string answer, TimeProvider timeProvider)
    {
        var faq = await DbContext.Set<Faq>()
            .AddAsync(Faq.Create(organizationId, question, answer, timeProvider));
        
        await DbContext.SaveChangesAsync();
        
        return faq.Entity;   
    }
    
    protected async Task<TemplateAnswer> CreateTemplateAnswer(Guid organizationId, string title, string text, TimeProvider timeProvider)
    {
        var templateAnswer = await DbContext.Set<TemplateAnswer>()
            .AddAsync(TemplateAnswer.Create(organizationId, title, text, timeProvider));
        
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

    protected async Task<SupportAgentInvite> CreateSupportAgentInvite(string email, Guid organizationId, TimeProvider timeProvider)
    {
        var invite = await DbContext.Set<SupportAgentInvite>()
            .AddAsync(SupportAgentInvite.Create(email, organizationId, timeProvider));
        
        await DbContext.SaveChangesAsync();
        
        return invite.Entity;
    }
}
