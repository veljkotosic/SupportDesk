using System.Net.Sockets;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;
using SupportDesk.Infrastructure.Elasticsearch;
using SupportDesk.Infrastructure.Elasticsearch.Initialization;
using SupportDesk.Infrastructure.Elasticsearch.Services;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class ElasticsearchRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskElasticsearch(IConfiguration configuration)
        {
            var elasticsearchUrl = configuration.GetEnvElasticsearchUrl();

            var elasticsearchUri = new Uri(elasticsearchUrl);
            var nodePool = new SingleNodePool(elasticsearchUri);

            var invoker = new HttpRequestInvoker((_, _) =>
            {
                return new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = TimeSpan.FromSeconds(15),
                    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                    
                    ConnectTimeout = TimeSpan.FromSeconds(2),
                    
                    KeepAlivePingDelay = TimeSpan.FromSeconds(5),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(2),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                    
                    EnableMultipleHttp2Connections = true,
                    
                    ConnectCallback = async (context, cancellationToken) =>
                    {
                        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                        {
                            NoDelay = true
                        };
                        
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 5);
                        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                        socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
                        
                        await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);
                        
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                };
            });

            var settings = new ElasticsearchClientSettings(nodePool, invoker)
                .DefaultIndex(ElasticsearchIndices.Tickets)
                .RequestTimeout(TimeSpan.FromSeconds(30))
                .DisableAutomaticProxyDetection(true);

            var client = new ElasticsearchClient(settings);

            services.AddSingleton(client);

            services.AddSingleton<ElasticsearchIndexInitializer>();

            services.AddScoped<ITicketReadModelService, ElasticsearchTicketReadModelService>();
            services.AddScoped<IUserReadModelService, ElasticsearchUserReadModelService>();
            services.AddScoped<IOrganizationReadModelService, ElasticsearchOrganizationReadModelService>();

            return services;
        }

        public IServiceCollection AddSupportDeskElasticsearchBackfill()
        {
            services.AddSingleton<ElasticsearchBackfillService>();
            services.AddHostedService(sp => sp.GetRequiredService<ElasticsearchBackfillService>());
            
            return services;
        }
    }
}