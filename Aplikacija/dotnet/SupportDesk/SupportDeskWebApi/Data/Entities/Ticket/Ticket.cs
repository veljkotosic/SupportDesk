using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;

namespace SupportDeskWebApi.Data.Entities.Ticket;

public class Ticket : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;
    
    public Guid CustomerId { get; set; }
    public User.User Customer { get; set; } = null!;
    
    public Guid SupportAgentId { get; set; }
    public User.User SupportAgent { get; set; } = null!;
    
    public Guid CategoryId { get; set; }
    public Category.Category Category { get; set; } = null!;
    
    public TicketStatus Status { get; set; }
    
    public DateTime OpenedAt { get; set; }
    
    public DateTime AssignedAt { get; set; }
    
    public DateTime ClosedAt { get; set; }
    
    public TicketFeedback Feedback { get; set; }

    public List<Note.Note> Notes { get; set; } = [];
    
    public List<Message.Message> Messages { get; set; } = [];
}