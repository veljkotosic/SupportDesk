using System.Text.Json;
using Asp.Versioning;
using DotNetEnv;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.WebApi.BackgroundServices.RealtimeUpdates;
using SupportDesk.WebApi.ExceptionHandlers;
using SupportDesk.WebApi.Filters;
using SupportDesk.WebApi.Hubs;

Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RefreshTokenExceptionHandler>();
builder.Services.AddExceptionHandler<PermissionExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<InternalExceptionHandler>();

builder.Services.AddSupportDeskObservability("SupportDesk.WebApi", builder.Configuration);
builder.Services.AddSupportDeskWebApi(builder.Configuration);

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHostedService<RabbitMqRealtimeUpdateConsumerService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AtLeastOneFieldRequiredRequestFilter>();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
    
app.MapHub<CustomerDashboardHub>("hubs/customerDashboardHub");
app.MapHub<OrganizationDashboardHub>("hubs/organizationDashboardHub");
app.MapHub<TicketHub>("hubs/ticketHub");

app.Run();