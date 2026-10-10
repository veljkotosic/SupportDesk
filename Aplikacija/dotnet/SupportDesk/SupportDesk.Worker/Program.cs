using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Infrastructure.Persistence.Database;
using SupportDesk.Worker.BackgroundServices.DomainEventHandling;

Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5013);
});

builder.Services.AddSupportDeskObservability("SupportDesk.Worker", builder.Configuration);
builder.Services.AddSupportDeskWorker(builder.Configuration);

builder.Services.AddHostedService<RabbitMqDomainEventConsumerService>();

var app = builder.Build();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
    _ = dbContext.Model; 
}

app.Run();