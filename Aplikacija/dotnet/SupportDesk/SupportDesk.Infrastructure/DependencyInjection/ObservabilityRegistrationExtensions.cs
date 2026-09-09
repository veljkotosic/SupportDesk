using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;

namespace SupportDesk.Infrastructure.DependencyInjection;

public static class ObservabilityRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupportDeskObservability(string serviceName, IConfiguration configuration)
        {
            var otlpEndpoint = new Uri(configuration.GetEnvOtelExporterOtlpEndpoint());
            
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0");

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
                builder.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
                builder.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);

                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;
                    options.AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri($"{otlpEndpoint}/v1/logs");
                        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });
            });

            services.AddOpenTelemetry()
                .WithTracing(provider =>
                {
                    provider
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(serviceName)
                        .AddOtlpExporter(exporter =>
                        {
                            exporter.Endpoint = new Uri($"{otlpEndpoint}/v1/traces");
                            exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                        });
                })
                .WithMetrics(provider =>
                {
                    provider
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(exporter =>
                        {
                            exporter.Endpoint = new Uri($"{otlpEndpoint}/v1/metrics");
                            exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                        });
                });
            
            return services;
        }
    }
}