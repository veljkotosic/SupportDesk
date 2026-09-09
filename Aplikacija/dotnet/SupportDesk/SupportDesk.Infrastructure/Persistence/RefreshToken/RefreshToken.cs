using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Infrastructure.Persistence.RefreshToken;

public sealed class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Token { get; set; }
    
    public Guid UserId { get; set; }
    
    public DateTime ExpiresAt { get; set; }

    public Application.Common.Auth.RefreshToken ToModel()
    {
        return new Application.Common.Auth.RefreshToken(Token, UserId, ExpiresAt);
    }
}