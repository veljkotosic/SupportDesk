using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class CacheRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskCache()
        {
            services.AddFusionCache();

            return services;
        }
    }
}
