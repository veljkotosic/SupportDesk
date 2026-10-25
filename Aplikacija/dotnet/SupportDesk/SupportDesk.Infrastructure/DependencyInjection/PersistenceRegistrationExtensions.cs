using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
            var writeConnectionString = configuration.GetEnvWriteConnectionString();
            var writeDataSource = NpgsqlDataSource.Create(writeConnectionString);

            services.AddSingleton(writeDataSource);

            services.AddDbContext<SupportDeskDbContext>((sp, options) =>
            {
                options.UseNpgsql(writeDataSource, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(SupportDeskDbContext).Assembly.FullName);
                });
            });

            var readConnectionString = configuration.GetEnvReadConnectionString();
            var readDataSource = NpgsqlDataSource.Create(readConnectionString);

            services.AddSingleton<IReadDbConnectionFactory>(sp => new ReadReplicaConnectionFactory(readDataSource));
            
            services.AddScoped<IApplicationDbContext, SupportDeskDbContext>();

            services.AddTransient<IUnitOfWork, EfUnitOfWork>();

            services.AddSupportDeskRepositories();

            services.AddTransient<IRefreshTokenManager, RefreshTokenManager>();
        
            return services;
        }
    }
}