using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Infrastructure.Persistence.Entities.Organizations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        
        builder.HasKey(organization => organization.Id);
        
        builder.Property(organization => organization.Id)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(organization => organization.Name)
            .HasConversion(
                name => name.NameValue,
                value => new OrganizationName(value))
            .IsRequired();

        builder.Property(organization => organization.Status)
            .IsRequired();
        
        builder.Property(organization => organization.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.Property(organization => organization.DeletedAt)
            .HasConversion(
                deletedAt => deletedAt != null ? deletedAt.DeletedAtValue : null,
                value => value.HasValue ? new DeletedAt(value.Value) : null)
            .IsRequired(false);
        
        builder.HasMany<User>()
            .WithOne()
            .HasForeignKey(user => user.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<TemplateAnswer>()
            .WithOne()
            .HasForeignKey(templateAnswer => templateAnswer.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Faq>()
            .WithOne()
            .HasForeignKey(faq => faq.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Category>()
            .WithOne()
            .HasForeignKey(category => category.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Note>()
            .WithOne()
            .HasForeignKey(note => note.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Message>()
            .WithOne()
            .HasForeignKey(message => message.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<Ticket>()
            .WithOne()
            .HasForeignKey(ticket => ticket.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<SupportAgentInvite>()
            .WithOne()
            .HasForeignKey(supportAgentInvite => supportAgentInvite.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany<TicketNotification>()
            .WithOne()
            .HasForeignKey(ticketNotification => ticketNotification.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}