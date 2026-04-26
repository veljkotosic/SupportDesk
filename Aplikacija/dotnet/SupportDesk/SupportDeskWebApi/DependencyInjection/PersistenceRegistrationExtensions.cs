using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.DependencyInjection.Configuration;

namespace SupportDeskWebApi.DependencyInjection;

public static class PersistenceRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskPersistence(IConfiguration configuration)
        {
            services.AddDbContext<SupportDeskDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetEnvConnectionString(), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(SupportDeskDbContext).Assembly.FullName);
                });
            });

            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            services.AddSupportDeskRepositories(configuration);
            
            
            
            return services;
        }
    }
}