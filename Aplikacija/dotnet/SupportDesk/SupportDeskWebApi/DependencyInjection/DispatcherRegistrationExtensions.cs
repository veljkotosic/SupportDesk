using SupportDeskWebApi.Dispatcher;

namespace SupportDeskWebApi.DependencyInjection;

public static class DispatcherRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskDispatcher()
        {
            services.AddTransient<IDispatcher, Dispatcher.Dispatcher>();
            
            return services;
        }
    }
}