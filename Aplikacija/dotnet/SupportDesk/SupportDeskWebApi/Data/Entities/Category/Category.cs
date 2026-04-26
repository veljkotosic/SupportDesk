using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Category;

public class Category : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string Description { get; set; } = null!;
    
    public List<Ticket.Ticket> Tickets { get; set; } = [];
}