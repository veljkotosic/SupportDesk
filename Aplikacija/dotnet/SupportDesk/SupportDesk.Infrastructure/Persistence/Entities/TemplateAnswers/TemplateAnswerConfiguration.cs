using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.TemplateAnswer.Options;
using SupportDesk.Domain.Models.TemplateAnswer.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.TemplateAnswers;

public class TemplateAnswerConfiguration : IEntityTypeConfiguration<TemplateAnswer>
{
    private readonly SupportDeskDbContext _context;

    public TemplateAnswerConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<TemplateAnswer> builder)
    {
        builder.ToTable("TemplateAnswers");
        
        builder.HasKey(templateAnswer => templateAnswer.Id);
        
        builder.Property(templateAnswer => templateAnswer.Id)
            .HasConversion(
                templateAnswerId => templateAnswerId.IdValue,
                value => new TemplateAnswerId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(templateAnswer => templateAnswer.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(templateAnswer => templateAnswer.Title)
            .HasConversion(
                title => title.TitleValue,
                value => new TemplateAnswerTitle(value))
            .HasMaxLength(TemplateAnswerOptionsDefaults.TitleMaximumLength)
            .IsRequired();
        
        builder.Property(templateAnswer => templateAnswer.Text)
            .HasConversion(
                text => text.TextValue,
                value => new TemplateAnswerText(value))
            .HasMaxLength(TemplateAnswerOptionsDefaults.TextMaximumLength)
            .IsRequired();
        
        builder.Property(templateAnswer => templateAnswer.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();       
        
        builder.Property(templateAnswer => templateAnswer.DeletedAt)
            .HasConversion<DateTime?>(
                deletedAt => deletedAt != null ? deletedAt.DeletedAtValue : null,
                value => value.HasValue ? new DeletedAt(value.Value) : null)
            .IsRequired(false);      
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(templateAnswer => templateAnswer.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(QueryFilterKeys.TenantIsolationFilter, 
            templateAnswer => templateAnswer.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
        
        builder.HasQueryFilter(QueryFilterKeys.SoftDeleteFilter,
            templateAnswer => templateAnswer.DeletedAt == null);
    }
}