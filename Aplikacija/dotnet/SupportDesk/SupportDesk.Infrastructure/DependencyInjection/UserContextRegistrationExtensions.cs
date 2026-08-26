using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Infrastructure.Auth.TenantContext;
using SupportDesk.Infrastructure.Auth.UserContext;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class UserContextRegistrationExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpUserContext()
        {
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContext, HttpUserContext>();
            services.AddScoped<ITenantContext, HttpTenantContext>();
        
            return services;
        }
    }
}