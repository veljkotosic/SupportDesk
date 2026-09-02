using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportDesk.Infrastructure.Messaging.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        
        builder.HasKey(inboxMessage => inboxMessage.Id);

        builder.Property(inboxMessage => inboxMessage.OutboxMessageId)
            .IsRequired();       
        
        builder.Property(inboxMessage => inboxMessage.HandlerType)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(inboxMessage => inboxMessage.Payload)
            .IsRequired();

        builder.Property(inboxMessage => inboxMessage.UserId)
            .IsRequired(false);
        
        builder.Property(inboxMessage => inboxMessage.OrganizationId)
            .IsRequired(false);       
        
        builder.Property(inboxMessage => inboxMessage.ReceivedOnUtc)
            .IsRequired();
        
        builder.Property(inboxMessage => inboxMessage.ProcessedOnUtc)
            .IsRequired(false);
        
        builder.Property(inboxMessage => inboxMessage.Error)
            .IsRequired(false);
    }
}