using System.ComponentModel.DataAnnotations;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.Organization;

public class Organization : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } = null!;
    
    public OrganizationStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime DeletedAt { get; set; }

    public List<User.User> Users { get; set; } = [];
    
    public List<TemplateAnswer.TemplateAnswer> TemplateAnswers { get; set; } = [];
    
    public List<Faq.Faq> Faqs { get; set; } = [];
    
    public List<Category.Category> Categories { get; set; } = [];
    
    public List<Note.Note> Notes { get; set; } = [];
    
    public List<Message.Message> Messages { get; set; } = [];
    
    public List<Ticket.Ticket> Tickets { get; set; } = [];
    
    public List<SupportAgentInviteCode.SupportAgentInviteCode> SupportAgentInviteCodes { get; set; } = [];
    
    public List<TicketNotification.TicketNotification> TicketNotifications { get; set; } = [];
}