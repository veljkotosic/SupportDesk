using System.Net;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.ReadModels;
using SupportDesk.Application.ReadModels;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;
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

            var customHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),
                PooledConnectionLifetime = TimeSpan.FromHours(1),
                ConnectTimeout = TimeSpan.FromSeconds(15),
                MaxConnectionsPerServer = 1000,          
                EnableMultipleHttp2Connections = false  
            };

            var customInvoker = new HttpRequestInvoker((_, _) => customHandler);
            
            var settings = new ElasticsearchClientSettings(nodePool, customInvoker)
                .DefaultIndex(ElasticsearchIndices.Tickets)
                .RequestTimeout(TimeSpan.FromSeconds(30))
                .DisableAutomaticProxyDetection(true)
                .DisableMetaHeader()
                .ConnectionLimit(1000); 

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


