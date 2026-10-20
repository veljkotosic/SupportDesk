using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class HealthCheckRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskWebApiHealthChecks(IConfiguration configuration)
        {
            services.AddRabbitMqHealthCheckConnection(configuration);

            services.AddHealthChecks()
                .AddDatabaseReadinessCheck()
                .AddRabbitMqReadinessCheck();
            
            return services;
        }

        public IServiceCollection AddSupportDeskOutboxProcessorHealthChecks(IConfiguration configuration)
        {
            services.AddRabbitMqHealthCheckConnection(configuration);

            services.AddHealthChecks()
                .AddDatabaseReadinessCheck()
                .AddRabbitMqReadinessCheck();
            
            return services;
        }
        
        public IServiceCollection AddSupportDeskWorkerHealthChecks(IConfiguration configuration)
        {
            services.AddRabbitMqHealthCheckConnection(configuration);

            services.AddHealthChecks()
                .AddDatabaseReadinessCheck()
                .AddRabbitMqReadinessCheck();
            
            return services;
        }

        private IServiceCollection AddRabbitMqHealthCheckConnection(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetEnvRabbitMqAmqpUri()),
                AutomaticRecoveryEnabled = true,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
                HandshakeContinuationTimeout = TimeSpan.FromSeconds(3),
                ContinuationTimeout = TimeSpan.FromSeconds(3)
            };

            var connectionTask = factory.CreateConnectionAsync();

            services.AddSingleton(connectionTask);

            return services;
        }
    }

    extension(IHealthChecksBuilder builder)
    {
        private IHealthChecksBuilder AddDatabaseReadinessCheck()
        {
            return builder.AddDbContextCheck<SupportDeskDbContext>(name: "database");
        }

        private IHealthChecksBuilder AddRabbitMqReadinessCheck()
        {
            return builder.AddRabbitMQ(
                factory: sp => sp.GetRequiredService<Task<IConnection>>(),
                name: "rabbitmq",
                timeout: TimeSpan.FromSeconds(5));
        }
    }
}