using System.Reflection;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.DependencyInjection;

public static class RequestHandlerRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskRequestHandlers(Assembly assembly)
        {
            var handlerInterfaces = new[]
            {
                typeof(IRequestHandler<>),
                typeof(IRequestHandler<,>),
            };
            
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false });
            
            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsGenericType)
                        continue;

                    var genericDef = @interface.GetGenericTypeDefinition();

                    if (!handlerInterfaces.Contains(genericDef))
                        continue;
                
                    services.AddTransient(@interface, type);
                }
            }

            return services;
        }
    }
}