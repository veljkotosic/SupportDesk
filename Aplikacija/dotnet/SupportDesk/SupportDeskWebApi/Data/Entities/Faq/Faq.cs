using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Faq;

public class Faq : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Question { get; set; } = null!;

    [Required]
    [MaxLength(400)]
    public string Answer { get; set; } = null!;
}