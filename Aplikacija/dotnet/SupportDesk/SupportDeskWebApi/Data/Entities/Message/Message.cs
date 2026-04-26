using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Message;

public class Message : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;
    
    public Guid TicketId { get; set; }
    public Ticket.Ticket Ticket { get; set; } = null!;
    
    public Guid SenderId { get; set; }
    public User.User Sender { get; set; } = null!;

    [Required]
    [MaxLength(400)]
    public string Text { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}