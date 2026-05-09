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
        
        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => c.OrganizationId == _userContext.GetCurrentUsersOrganizationId());
        
        modelBuilder.Entity<TemplateAnswer>()
            .HasQueryFilter(ta => ta.OrganizationId == _userContext.GetCurrentUsersOrganizationId());
        
        modelBuilder.Entity<Faq>()
            .HasQueryFilter(f => f.OrganizationId == _userContext.GetCurrentUsersOrganizationId());
    }
}