using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Infrastructure.Dispatcher;
using SupportDesk.Infrastructure.Event;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class DispatcherRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCommandDispatcher()
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            
            return services;
        }
        
        public IServiceCollection AddQueryDispatcher()
        {
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
            
            return services;
        }
        
        public IServiceCollection AddDomainEventDispatcher()
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            services.AddScoped<IDomainEventCollector, DomainEventCollector>();
            
            return services;
        }
    }
}