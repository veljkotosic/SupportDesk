using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.UserContext;

namespace SupportDeskWebApi.DependencyInjection;

public static class UserContextRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskUserContext()
        {
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContext, HttpUserContext>();
        
            return services;
        }
    }
}