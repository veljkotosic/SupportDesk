using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Entities.Category;
using SupportDeskWebApi.Data.Entities.Faq;
using SupportDeskWebApi.Data.Entities.Message;
using SupportDeskWebApi.Data.Entities.Note;
using SupportDeskWebApi.Data.Entities.Organization;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode;
using SupportDeskWebApi.Data.Entities.TemplateAnswer;
using SupportDeskWebApi.Data.Entities.Ticket;
using SupportDeskWebApi.Data.Entities.TicketNotification;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Data.Database;

public class SupportDeskDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly IUserContext _userContext;
    
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<TemplateAnswer> TemplateAnswers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<SupportAgentInviteCode> SupportAgentInviteCodes { get; set; }
    public DbSet<TicketNotification> TicketNotifications { get; set; }
    
    public DbSet<RefreshToken.RefreshToken> RefreshTokens { get; set; }
    
    public SupportDeskDbContext(
        DbContextOptions<SupportDeskDbContext> options, 
        IUserContext userContext) 
        : base(options)
    {
        _userContext = userContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("pg_trgm");
        
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany(u => u.OpenedTickets)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.SupportAgent)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.SupportAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.OrganizationId, t.LastMessageAt, t.Id })
            .IsDescending(false, true, true);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.OrganizationId, t.Status, t.LastMessageAt, t.Id })
            .IsDescending(false, false, true, true);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.OrganizationId, t.Priority, t.LastMessageAt, t.Id })
            .IsDescending(false, false, true, true);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.CustomerId, t.LastMessageAt, t.Id })
            .IsDescending(false, true, true);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => new { t.CustomerId, t.Status, t.LastMessageAt, t.Id })
            .IsDescending(false, false, true, true);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Subject)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        modelBuilder.Entity<Organization>()
            .HasIndex(o => o.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        
        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => 
                (_userContext.GetCurrentUsersOrganizationId() != null && c.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) ||
                (_userContext.GetCurrentUsersOrganizationId() == null)); 

        modelBuilder.Entity<TemplateAnswer>()
            .HasQueryFilter(ta => 
                (_userContext.GetCurrentUsersOrganizationId() != null && ta.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) ||
                (_userContext.GetCurrentUsersOrganizationId() == null));

        modelBuilder.Entity<Faq>()
            .HasQueryFilter(f => 
                (_userContext.GetCurrentUsersOrganizationId() != null && f.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) ||
                (_userContext.GetCurrentUsersOrganizationId() == null));
        
        modelBuilder.Entity<TicketNotification>()
            .HasQueryFilter(tn => 
                (_userContext.GetCurrentUsersOrganizationId() != null && tn.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) || 
                (tn.Ticket.CustomerId == _userContext.GetCurrentUserId()));
        
        modelBuilder.Entity<Ticket>()
            .HasQueryFilter(t => 
                (_userContext.GetCurrentUsersOrganizationId() != null && t.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) || 
                (t.CustomerId == _userContext.GetCurrentUserId()));

        modelBuilder.Entity<Message>()
            .HasQueryFilter(m => 
                (_userContext.GetCurrentUsersOrganizationId() != null && m.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) || 
                (m.Ticket.CustomerId == _userContext.GetCurrentUserId()));
        
        modelBuilder.Entity<Note>()
            .HasQueryFilter(n => 
                (_userContext.GetCurrentUsersOrganizationId() != null && n.OrganizationId == _userContext.GetCurrentUsersOrganizationId()) || 
                (n.Ticket.CustomerId == _userContext.GetCurrentUserId()));
    }
}
