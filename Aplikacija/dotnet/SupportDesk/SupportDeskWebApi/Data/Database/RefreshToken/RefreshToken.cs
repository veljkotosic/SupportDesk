using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Data.Database.RefreshToken;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public required string Token { get; set; }
    
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime ExpiresAt { get; set; }
}