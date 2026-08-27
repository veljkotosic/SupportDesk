using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Event;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class SupportDeskRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskWebApi(IConfiguration configuration)
        {
            services.AddSingleton<TimeProvider>();
            
            services.AddSupportDeskHandlers(typeof(IUseCase).Assembly);
            services.AddSupportDeskDomainEventHandlers(typeof(IDomainEventHandler<>).Assembly);
            
            services.AddCommandDispatcher();
            services.AddQueryDispatcher();
            services.AddDomainEventDispatcher();
            
            services.AddSupportDeskPersistence(configuration);

            services.AddSupportDeskHealthChecks();
            
            services.AddSupportDeskAuth(configuration);
            services.AddHttpUserContext();
            
            return services;
        }
    }
}