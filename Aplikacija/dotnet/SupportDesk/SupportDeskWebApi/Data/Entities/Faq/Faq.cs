using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Faq;

public class Faq : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}