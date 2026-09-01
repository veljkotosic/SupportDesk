using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Infrastructure.Auth.TenantContext;
using SupportDesk.Infrastructure.Auth.UserContext;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class ExecutionContextRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpExecutionContext()
        {
            services.AddHttpContextAccessor();

            services.AddScoped<IUserContext, HttpUserContext>();
            services.AddScoped<ITenantContext, HttpTenantContext>();
        
            return services;
        }

        public IServiceCollection AddWorkerExecutionContext()
        {
            services.AddScoped<WorkerUserContext>();
            services.AddScoped<IUserContext>(sp => sp.GetRequiredService<WorkerUserContext>());
            services.AddScoped<IUserContextSetter>(sp => sp.GetRequiredService<WorkerUserContext>());
            
            services.AddScoped<WorkerTenantContext>();
            services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<WorkerTenantContext>());
            services.AddScoped<ITenantContextSetter>(sp => sp.GetRequiredService<WorkerTenantContext>());
            
            return services;
        }
    }
}