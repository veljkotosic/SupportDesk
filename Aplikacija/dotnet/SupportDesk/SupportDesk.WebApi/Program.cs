using System.Text.Json;
using Asp.Versioning;
using DotNetEnv;
using Scalar.AspNetCore;
using SupportDesk.Infrastructure.DependencyInjection;
using SupportDesk.WebApi.BackgroundServices.RealtimeUpdates;
using SupportDesk.WebApi.ExceptionHandlers;
using SupportDesk.WebApi.Filters;
using SupportDesk.WebApi.Hubs;

Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(config =>
    {
        config.AddConsole();
        config.AddDebug();
    })
    .AddHttpLogging();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "SupportDesk API v1";
        document.Info.Version = "v1";
        document.Info.Description = "SupportDesk Web API";
        return Task.CompletedTask;
    });
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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SupportDesk API Documentation");
    });
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();
    
app.MapHub<CustomerDashboardHub>("hubs/customerDashboardHub");
app.MapHub<OrganizationDashboardHub>("hubs/organizationDashboardHub");
app.MapHub<TicketHub>("hubs/ticketHub");

app.UseHttpLogging();

app.Run();