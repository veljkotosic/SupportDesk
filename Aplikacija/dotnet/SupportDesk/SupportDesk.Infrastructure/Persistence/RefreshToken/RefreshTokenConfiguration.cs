using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SupportDesk.Infrastructure.Persistence.RefreshToken;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        
        builder.HasKey(refreshToken => refreshToken.Id);
        
        builder.Property(refreshToken => refreshToken.Token)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(refreshToken => refreshToken.UserId)
            .IsRequired();
            
        builder.Property(refreshToken => refreshToken.ExpiresAt)
            .IsRequired();

        builder.HasIndex(refreshToken => refreshToken.Token)
            .IsUnique();

        builder.HasIndex(refreshToken => refreshToken.UserId);
    }
}
