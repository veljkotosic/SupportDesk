using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Faq.Options;
using SupportDesk.Domain.Models.Faq.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Faqs;

public class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    private readonly SupportDeskDbContext _context;

    public FaqConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs");
        
        builder.HasKey(faq => faq.Id);
        
        builder.Property(faq => faq.Id)
            .HasConversion(
                faqId => faqId.IdValue,
                value => new FaqId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(faq => faq.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();

        builder.Property(faq => faq.Question)
            .HasConversion(
                question => question.QuestionValue,
                value => new FaqQuestion(value))
            .HasMaxLength(FaqOptionsDefaults.QuestionMaximumLength)
            .IsRequired();
        
        builder.Property(faq => faq.Answer)
            .HasConversion(
                answer => answer.AnswerValue,
                value => new FaqAnswer(value))
            .HasMaxLength(FaqOptionsDefaults.AnswerMaximumLength)
            .IsRequired();
        
        builder.Property(faq => faq.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();       
        
        builder.Property(faq => faq.DeletedAt)
            .HasConversion<DateTime?>(
                deletedAt => deletedAt != null ? deletedAt.DeletedAtValue : null,
                value => value.HasValue ? new DeletedAt(value.Value) : null)
            .IsRequired(false);       
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(faq => faq.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(
            "TenantIsolationFilter", 
            faq => faq.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
        
        builder.HasQueryFilter(
            "SoftDeleteFilter",
            faq => faq.DeletedAt == null);
    }
}