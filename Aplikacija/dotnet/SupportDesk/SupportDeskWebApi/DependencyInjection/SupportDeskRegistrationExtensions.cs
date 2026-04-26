namespace SupportDeskWebApi.DependencyInjection;

public static class SupportDeskRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDesk(IConfiguration configuration)
        {
            services.AddSupportDeskPersistence(configuration);
            
            return services;
        }
    }
}