using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportDesk.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        
        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.EventType)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(outboxMessage => outboxMessage.Payload)
            .IsRequired();
        
        builder.Property(outboxMessage => outboxMessage.UserId)
            .IsRequired(false);
        
        builder.Property(outboxMessage => outboxMessage.OrganizationId)
            .IsRequired(false); 
        
        builder.Property(outboxMessage => outboxMessage.OccurredOnUtc)
            .IsRequired();
        
        builder.Property(outboxMessage => outboxMessage.ProcessedOnUtc)
            .IsRequired(false);
        
        builder.Property(outboxMessage => outboxMessage.Error)
            .IsRequired(false);
    }
}