using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;

namespace SupportDeskWebApi.Data.Entities.TicketNotification;

public class TicketNotification : IEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    public Organization.Organization Organization { get; set; } = null!;
    
    public Guid TicketId { get; set; }
    public Ticket.Ticket Ticket { get; set; } = null!;
    
    [MaxLength(400)]
    public string Text { get; set; } = null!;
    
    public TicketNotificationStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
}