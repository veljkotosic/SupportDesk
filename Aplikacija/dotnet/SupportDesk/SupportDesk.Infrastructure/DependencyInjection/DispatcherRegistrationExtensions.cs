using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Infrastructure.Dispatcher;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class DispatcherRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskCommandDispatcher()
        {
            services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            
            return services;
        }
        
        public IServiceCollection AddSupportDeskQueryDispatcher()
        {
            services.AddScoped<IQueryDispatcher, QueryDispatcher>();
            
            return services;
        }
    }
}