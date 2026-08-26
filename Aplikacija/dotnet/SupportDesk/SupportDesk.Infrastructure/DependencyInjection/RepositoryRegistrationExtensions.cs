using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Domain.Models.Category.Repository;
using SupportDesk.Domain.Models.Faq.Repository;
using SupportDesk.Domain.Models.Message.Repository;
using SupportDesk.Domain.Models.Note.Repository;
using SupportDesk.Domain.Models.Organization.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.Repository;
using SupportDesk.Domain.Models.TemplateAnswer.Repository;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.TicketNotification.Repository;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Infrastructure.Persistence.Entities.Categories;
using SupportDesk.Infrastructure.Persistence.Entities.Faqs;
using SupportDesk.Infrastructure.Persistence.Entities.Messages;
using SupportDesk.Infrastructure.Persistence.Entities.Notes;
using SupportDesk.Infrastructure.Persistence.Entities.Organizations;
using SupportDesk.Infrastructure.Persistence.Entities.SupportAgentInvites;
using SupportDesk.Infrastructure.Persistence.Entities.TemplateAnswers;
using SupportDesk.Infrastructure.Persistence.Entities.TicketNotifications;
using SupportDesk.Infrastructure.Persistence.Entities.Tickets;
using SupportDesk.Infrastructure.Persistence.Entities.Users;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class RepositoryRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskRepositories()
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ISupportAgentInviteRepository, SupportAgentInviteRepository>();
            services.AddScoped<ITemplateAnswerRepository, TemplateAnswerRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketNotificationRepository, TicketNotificationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            
            return services;
        }
    }
}