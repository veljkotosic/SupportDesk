using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.SupportAgentInvites;

public class SupportAgentInviteConfiguration : IEntityTypeConfiguration<SupportAgentInvite>
{
    private readonly SupportDeskDbContext _context;

    public SupportAgentInviteConfiguration(SupportDeskDbContext context)
    {
        _context = context;
    }

    public void Configure(EntityTypeBuilder<SupportAgentInvite> builder)
    {
        builder.ToTable("SupportAgentInvites");
        
        builder.HasKey(supportAgentInvite => supportAgentInvite.Id);
        
        builder.Property(supportAgentInvite => supportAgentInvite.Id)
            .HasConversion(
                supportAgentInviteId => supportAgentInviteId.IdValue,
                value => new SupportAgentInviteId(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(supportAgentInvite => supportAgentInvite.OrganizationId)
            .HasConversion(
                organizationId => organizationId.IdValue,
                value => new OrganizationId(value))
            .IsRequired();

        builder.Property(supportAgentInvite => supportAgentInvite.Code)
            .HasConversion(
                code => code.CodeValue,
                value => new SupportAgentInviteCode(value))
            .IsRequired();
        
        builder.Property(supportAgentInvite => supportAgentInvite.Email)
            .HasConversion(
                email => email.EmailValue,
                value => new Email(value))
            .IsRequired();
        
        builder.Property(supportAgentInvite => supportAgentInvite.CreatedAt)
            .HasConversion(
                createdAt => createdAt.CreatedAtValue,
                value => new CreatedAt(value))
            .IsRequired();

        builder.Property(supportAgentInvite => supportAgentInvite.Status)
            .IsRequired();

        builder.Property(supportAgentInvite => supportAgentInvite.UsedAt)
            .HasConversion(
                usedAt => usedAt != null ? usedAt.UsedAtValue : null,
                value => value != null ? new SupportAgentInviteUsedAt(value) : null)
            .IsRequired(false);
        
        builder.Property(supportAgentInvite => supportAgentInvite.ExpiresAt)
            .HasConversion(
                expiresAt => expiresAt.ExpiresAtValue,
                value => new SupportAgentInviteExpiresAt(value))
            .IsRequired();
        
        builder.Property(supportAgentInvite => supportAgentInvite.RevokedAt)
            .HasConversion(
                revokedAt => revokedAt != null ? revokedAt.RevokedAtValue : null,
                value => value != null ? new SupportAgentInviteRevokedAt(value) : null)
            .IsRequired(false);
        
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(supportAgentInvite => supportAgentInvite.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(supportAgentInvite => 
            supportAgentInvite.OrganizationId == (_context.OrganizationId != null ? new OrganizationId(_context.OrganizationId.Value) : null));
    }
}