using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Options;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    private readonly SupportDeskDbContext _context;

    public TicketConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        
        builder.HasKey(ticket => ticket.Id);
        
        builder.Property(ticket => ticket.Id)
            .HasConversion(
            ticketId => ticketId.IdValue,
            value => new TicketId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(ticket => ticket.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(ticket => ticket.CustomerId)
            .HasConversion(
                customerId => customerId.IdValue,
                value => new UserId(value))
            .IsRequired();
        
        builder.Property(ticket => ticket.SupportAgentId)
            .HasConversion<Guid?>(
                supportAgentId => supportAgentId != null ? supportAgentId.IdValue : null,
                value => value != null ? new UserId((Guid)value) : null)
            .IsRequired(false);
        
        builder.Property(ticket => ticket.CategoryId)
            .HasConversion(
                categoryId => categoryId.IdValue,
                value => new CategoryId(value))
            .IsRequired();
        
        builder.Property(ticket => ticket.Status)
            .IsRequired();

        builder.Property(ticket => ticket.Priority)
            .IsRequired();

        builder.Property(ticket => ticket.Feedback)
            .IsRequired();
        
        builder.Property(ticket => ticket.Subject)
            .HasConversion(
                subject => subject.SubjectValue,
                value => new TicketSubject(value))
            .HasMaxLength(TicketOptionsDefaults.SubjectMaximumLength)
            .IsRequired();
        
        builder.Property(ticket => ticket.OpenedAt)
            .HasConversion(
                openedAt => openedAt.OpenedAtValue,
                value => new TicketOpenedAt(value))
            .IsRequired();
        
        builder.Property(ticket => ticket.AssignedAt)
            .HasConversion<DateTime?>(
                assignedAt => assignedAt != null ? assignedAt.AssignedAtValue : null,
                value => value.HasValue ? new TicketAssignedAt(value.Value) : null)
            .IsRequired(false);
        
        builder.Property(ticket => ticket.ClosedAt)
            .HasConversion<DateTime?>(
                closedAt => closedAt != null ? closedAt.ClosedAtValue : null,
                value => value.HasValue ? new TicketClosedAt(value.Value) : null)
            .IsRequired(false);
        
        builder.Property(ticket => ticket.LastMessageAt)
            .HasConversion<DateTime?>(
                lastMessageAt => lastMessageAt != null ? lastMessageAt.LastMessageAtValue : null,
                value => value.HasValue ? new TicketLastMessageAt(value.Value) : null)
            .IsRequired(false);
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(ticket => ticket.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CustomerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ticket => ticket.SupportAgentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CategoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Note>()
            .WithOne()
            .HasForeignKey(note => note.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Message>()
            .WithOne()
            .HasForeignKey(message => message.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<TicketNotification>()
            .WithOne()
            .HasForeignKey(ticketNotification => ticketNotification.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
                
        builder.HasQueryFilter(ticket => 
            _context.OrganizationId == null || 
            ticket.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}