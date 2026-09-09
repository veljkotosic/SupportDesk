using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Messaging;
using SupportDesk.Infrastructure.Messaging;
using SupportDesk.Infrastructure.Messaging.RabbitMq;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class MessagingRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRabbitMqConnection()
        {
            services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            
            return services;
        }
        
        public IServiceCollection AddRabbitMqRealtimePublisher()
        {
            services.AddRabbitMqConnection();
            services.AddTransient<IRealtimePublisher, RabbitMqRealtimePublisher>();
            
            return services;
        }
        
        public IServiceCollection AddDomainEventPublisher()
        {
            services.AddRabbitMqConnection();
            services.AddTransient<IDomainEventPublisher, RabbitMqDomainEventPublisher>();
            
            return services;
        }
    }
}