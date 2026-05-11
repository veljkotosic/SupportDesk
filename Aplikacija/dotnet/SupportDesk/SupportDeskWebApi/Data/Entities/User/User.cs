using Microsoft.AspNetCore.Identity;
using SupportDeskWebApi.Data.Entities.Common;

namespace SupportDeskWebApi.Data.Entities.User;

public class User : IdentityUser<Guid>, IEntity
{
    public Guid? OrganizationId { get; set; }
    public Organization.Organization? Organization { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public List<Ticket.Ticket> OpenedTickets { get; set; } = [];
    public List<Ticket.Ticket> AssignedTickets { get; set; } = [];
    
    public List<Message.Message> Messages { get; set; } = [];
    
    public List<Note.Note> Notes { get; set; } = [];
}