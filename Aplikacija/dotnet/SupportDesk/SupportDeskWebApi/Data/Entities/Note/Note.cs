using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Note;

public class Note : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;
    
    public Guid TicketId { get; set; }
    public Ticket.Ticket Ticket { get; set; } = null!;

    [Required] 
    public string Text { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}