using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class HealthCheckRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskHealthChecks()
        {
            services.AddHealthChecks().AddDbContextCheck<SupportDeskDbContext>();
            
            return services;
        }
    }
}