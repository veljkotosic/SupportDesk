using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Worker.BackgroundServices.DomainEventHandling;

Env.TraversePath().NoClobber().Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSupportDeskObservability("SupportDesk.Worker", builder.Configuration);
builder.Services.AddSupportDeskWorker(builder.Configuration);

builder.Services.AddHostedService<RabbitMqDomainEventConsumerService>();

var host = builder.Build();

host.Run();