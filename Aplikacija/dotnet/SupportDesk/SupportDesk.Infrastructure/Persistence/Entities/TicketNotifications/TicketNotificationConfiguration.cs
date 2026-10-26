using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.TicketNotification.Options;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.TicketNotifications;

public class TicketNotificationConfiguration : IEntityTypeConfiguration<TicketNotification>
{
    private readonly SupportDeskDbContext _context;

    public TicketNotificationConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<TicketNotification> builder)
    {
        builder.ToTable("TicketNotifications");
        
        builder.HasKey(ticketNotification => ticketNotification.Id);
        
        builder.Property(ticketNotification => ticketNotification.Id)
            .HasConversion(
                ticketNotificationId => ticketNotificationId.IdValue,
                value => new TicketNotificationId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(ticketNotification => ticketNotification.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(ticketNotification => ticketNotification.TicketId)
            .HasConversion(
                ticketId => ticketId.IdValue,
                value => new TicketId(value))
            .IsRequired();
        
        builder.Property(ticketNotification => ticketNotification.Text)
            .HasConversion(
                text => text.TextValue,
                value => new TicketNotificationText(value))
            .HasMaxLength(TicketNotificationOptionsDefaults.TextMaximumLength)
            .IsRequired();

        builder.Property(ticketNotification => ticketNotification.Status)
            .IsRequired();
        
        builder.Property(ticketNotification => ticketNotification.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(ticketNotification => ticketNotification.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(ticketNotification => ticketNotification.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(ticketNotification => 
            ticketNotification.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}