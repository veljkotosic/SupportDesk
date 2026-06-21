using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.TemplateAnswer;

public class TemplateAnswer : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(400)]
    public string Text { get; set; } = null!;
}