using SupportDesk.Domain.Models.Category;
using SupportDesk.Domain.Models.Faq;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Note;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.TemplateAnswer;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Application.Abstract.Database;

public interface IApplicationDbContext
{
    IQueryable<Category> Categories { get; }
    IQueryable<Faq> Faqs { get; }
    IQueryable<Message> Messages { get; }
    IQueryable<Note> Notes { get; }
    IQueryable<Organization> Organizations { get; }
    IQueryable<SupportAgentInvite> SupportAgentInvites { get; }
    IQueryable<TemplateAnswer> TemplateAnswers { get; }
    IQueryable<Ticket> Tickets { get; }
    IQueryable<TicketNotification> TicketNotifications { get; }
    IQueryable<User> DomainUsers { get; }
}