using DotNetEnv;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.Infrastructure.Persistence.Database;
using SupportDesk.OutboxProcessor.BackgroundServices.OutboxProcessing;

Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5012);
});

builder.Services.AddSupportDeskObservability("SupportDesk.OutboxProcessor", builder.Configuration);
builder.Services.AddSupportDeskOutboxProcessor(builder.Configuration);

builder.Services.AddHostedService<OutboxProcessorService>();

var app = builder.Build();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();
    _ = dbContext.Model; 
}

app.Run();