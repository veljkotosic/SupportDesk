using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Database;
using SupportDesk.Domain.Abstract;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;
using SupportDesk.Infrastructure.Persistence.Database;
using SupportDesk.Infrastructure.Persistence.RefreshToken;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class PersistenceRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskPersistence(IConfiguration configuration)
        {
            services.AddDbContext<SupportDeskDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetEnvConnectionString(), npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(SupportDeskDbContext).Assembly.FullName);
                });
            });
            
            services.AddScoped<IApplicationDbContext, SupportDeskDbContext>();

            services.AddTransient<IUnitOfWork, EfUnitOfWork>();

            services.AddSupportDeskRepositories();

            services.AddTransient<IRefreshTokenManager, RefreshTokenManager>();
        
            return services;
        }
    }
}