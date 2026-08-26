using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Abstract.Event;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class HandlerRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskHandlers(Assembly assembly)
        {
            var handlerInterfaces = new[]
            {
                typeof(ICommandHandler<>),
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>),
            };
            
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false });
            
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    var genericDef = @interface.GetGenericTypeDefinition();

                    if (!handlerInterfaces.Contains(genericDef))
                    {
                        continue;
                    }
                
                    services.AddTransient(@interface, type);
                }
            }

            return services;
        }

        public IServiceCollection AddSupportDeskDomainEventHandlers(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false });
            
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    var genericDef = @interface.GetGenericTypeDefinition();

                    if (genericDef != typeof(IDomainEventHandler<>))
                    {
                        continue;
                    }
                
                    services.AddTransient(@interface, type);
                }
            }

            return services;
        }
    }
}