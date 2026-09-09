using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.User;
using SupportDesk.Infrastructure.Auth.Identity;
using SupportDesk.Infrastructure.Messaging.Inbox;
using SupportDesk.Infrastructure.Messaging.Outbox;
using SupportDesk.Infrastructure.Persistence.Entities.Categories;
using SupportDesk.Infrastructure.Persistence.Entities.Faqs;
using SupportDesk.Infrastructure.Persistence.Entities.Messages;
using SupportDesk.Infrastructure.Persistence.Entities.Notes;
using SupportDesk.Infrastructure.Persistence.Entities.Organizations;
using SupportDesk.Infrastructure.Persistence.Entities.SupportAgentInvites;
using SupportDesk.Infrastructure.Persistence.Entities.TemplateAnswers;
using SupportDesk.Infrastructure.Persistence.Entities.TicketNotifications;
using SupportDesk.Infrastructure.Persistence.Entities.Tickets;
using SupportDesk.Infrastructure.Persistence.Entities.Users;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class SupportDeskDbContext
    : IdentityDbContext<AppIdentityUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly IUserContext _userContext;
    private readonly ITenantContext _tenantContext;
    
    public SupportDeskDbContext(
        DbContextOptions<SupportDeskDbContext> options, 
        IUserContext userContext,
        ITenantContext tenantContext) 
        : base(options)
    {
        _userContext = userContext;
        _tenantContext = tenantContext;
    }

    public Guid? UserId => _userContext.TryGetCurrentUserId();
    public Guid? OrganizationId => _tenantContext.GetCurrentOrganizationId();
    
    public IQueryable<Category> Categories => Set<Category>();
    public IQueryable<Faq> Faqs => Set<Faq>();
    public IQueryable<Message> Messages => Set<Message>();
    public IQueryable<Note> Notes => Set<Note>();
    public IQueryable<Organization> Organizations => Set<Organization>();
    public IQueryable<SupportAgentInvite> SupportAgentInvites => Set<SupportAgentInvite>();
    public IQueryable<TemplateAnswer> TemplateAnswers => Set<TemplateAnswer>();
    public IQueryable<Ticket> Tickets => Set<Ticket>();
    public IQueryable<TicketNotification> TicketNotifications => Set<TicketNotification>();
    
    public IQueryable<User> DomainUsers => Set<User>();
    
    public DbSet<RefreshToken.RefreshToken> RefreshTokens { get; set; }
    
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new CategoryConfiguration(this));
        builder.ApplyConfiguration(new FaqConfiguration(this));
        builder.ApplyConfiguration(new MessageConfiguration(this));
        builder.ApplyConfiguration(new NoteConfiguration(this));
        builder.ApplyConfiguration(new OrganizationConfiguration());
        builder.ApplyConfiguration(new SupportAgentInviteConfiguration(this));
        builder.ApplyConfiguration(new TemplateAnswerConfiguration(this));
        builder.ApplyConfiguration(new TicketConfiguration(this));
        builder.ApplyConfiguration(new TicketNotificationConfiguration(this));

        builder.ApplyConfiguration(new UserConfiguration(this));
        
        builder.ApplyConfiguration(new OutboxMessageConfiguration());
        builder.ApplyConfiguration(new InboxMessageConfiguration());

    }
}