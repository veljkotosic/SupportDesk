using SupportDeskWebApi.Data.Entities.Category.Repository;
using SupportDeskWebApi.Data.Entities.Faq.Repository;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Note.Repository;
using SupportDeskWebApi.Data.Entities.Organization.Repository;
using SupportDeskWebApi.Data.Entities.TemplateAnswer.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.User.Repository;

namespace SupportDeskWebApi.DependencyInjection;

public static class RepositoryRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskRepositories(IConfiguration configuration)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ITicketRepository,  TicketRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<ITemplateAnswerRepository, TemplateAnswerRepository>();
            services.AddScoped<IFaqRepository, FaqRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            
            return services;
        }
    }
}