using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Message.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Messages;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    private readonly SupportDeskDbContext _context;

    public MessageConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        
        builder.HasKey(message => message.Id);
        
        builder.Property(message => message.Id)
            .HasConversion(
                messageId => messageId.IdValue,
                value => new MessageId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(message => message.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(message => message.TicketId)
            .HasConversion(
                ticketId => ticketId.IdValue,
                value => new TicketId(value))
            .IsRequired();
        
        builder.Property(message => message.SenderId)
            .HasConversion(
                senderId => senderId.IdValue,
                value => new UserId(value))
            .IsRequired();
        
        builder.Property(message => message.Text)
            .HasConversion(
                text => text.TextValue,
                value => new MessageText(value))
            .IsRequired();

        builder.Property(message => message.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(message => message.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(message => message.TicketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(message => 
            message.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}