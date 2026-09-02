using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.OutboxProcessor.BackgroundServices.OutboxProcessing;

Env.TraversePath().NoClobber().Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSupportDeskObservability("SupportDesk.OutboxProcessor", builder.Configuration);
builder.Services.AddSupportDeskOutboxProcessor(builder.Configuration);

builder.Services.AddHostedService<OutboxProcessorService>();

var host = builder.Build();

host.Run();