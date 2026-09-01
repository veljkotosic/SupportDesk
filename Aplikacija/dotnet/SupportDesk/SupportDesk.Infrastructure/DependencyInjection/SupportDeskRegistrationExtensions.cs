using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class SupportDeskRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskWebApi(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            
            services.AddSupportDeskCommandHandlers(typeof(ICommand).Assembly);
            services.AddSupportDeskQueryHandlers(typeof(IQuery<>).Assembly);
            services.AddSupportDeskDomainEventHandlers(typeof(IDomainEventHandler<>).Assembly);
            
            services.AddSupportDeskCommandDispatcher();
            services.AddSupportDeskQueryDispatcher();
            
            services.AddSupportDeskPersistence(configuration);
            
            services.AddRabbitMqConnection();

            services.AddSupportDeskHealthChecks();
            
            services.AddSupportDeskAuth(configuration);
            services.AddHttpExecutionContext();
            
            return services;
        }

        public IServiceCollection AddSupportDeskWorker(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            
            services.AddSupportDeskCommandHandlers(typeof(ICommand).Assembly);
            services.AddSupportDeskDomainEventHandlers(typeof(IDomainEventHandler<>).Assembly);

            services.AddSupportDeskCommandDispatcher();

            services.AddSupportDeskPersistence(configuration);

            services.AddWorkerExecutionContext();

            services.AddRabbitMqRealtimePublisher();
            
            return services;
        }

        public IServiceCollection AddSupportDeskOutboxProcessor(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);

            services.AddSupportDeskPersistence(configuration);

            services.AddWorkerExecutionContext();
            
            services.AddDomainEventPublisher();
            
            return services;
        }
    }
}