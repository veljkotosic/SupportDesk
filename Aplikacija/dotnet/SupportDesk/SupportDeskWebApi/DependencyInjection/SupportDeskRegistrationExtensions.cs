namespace SupportDeskWebApi.DependencyInjection;

public static class SupportDeskRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDesk(IConfiguration configuration)
        {
            services.AddSupportDeskRequestHandlers(typeof(SupportDeskRegistrationExtensions).Assembly);
            services.AddSupportDeskDispatcher();
            
            services.AddSupportDeskPersistence(configuration);
            
            services.AddSupportDeskAuth(configuration);
            services.AddSupportDeskUserContext();
            
            return services;
        }
    }
}