using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Category.Options;
using SupportDesk.Domain.Models.Category.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.Categories;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private readonly SupportDeskDbContext _context;

    public CategoryConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasConversion(
                categoryId => categoryId.IdValue, 
                value => new CategoryId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(category => category.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();
        
        builder.Property(category => category.Name)
            .HasConversion(
                name => name.NameValue,
                value => new CategoryName(value))
            .HasMaxLength(CategoryOptionsDefaults.NameMaximumLength)
            .IsRequired();
        
        builder.Property(category => category.Description)
            .HasConversion(
                desc => desc.DescriptionValue,
                value => new CategoryDescription(value))
            .HasMaxLength(CategoryOptionsDefaults.DescriptionMaximumLength)
            .IsRequired();
        
        builder.Property(category => category.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();
        
        builder.Property(category => category.DeletedAt)
            .HasConversion<DateTime?>(
                deletedAt => deletedAt != null ? deletedAt.DeletedAtValue : null,
                value => value.HasValue ? new DeletedAt(value.Value) : null)
            .IsRequired(false);
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(category => category.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(
            "TenantIsolationFilter", 
            category => category.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
        
        builder.HasQueryFilter(
            "SoftDeleteFilter",
            category => category.DeletedAt == null);
    }
}