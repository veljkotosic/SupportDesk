using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.SupportAgentInviteCode;

public class SupportAgentInviteCode : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid Code { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    public Guid OrganizationId { get; set; } 
    public Organization.Organization Organization { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public SupportAgentInviteCodeStatus Status { get; set; }
    
    public DateTime UsedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime RevokedAt { get; set; }
}