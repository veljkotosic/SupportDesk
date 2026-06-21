using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.DependencyInjection;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Middleware;

if (File.Exists("../../../.env"))
{
    Env.NoClobber().Load("../../../.env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new
            {
                field = entry.Key,
                message = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value." : error.ErrorMessage,
            }))
            .ToList();

        return new BadRequestObjectResult(new
        {
            status = StatusCodes.Status400BadRequest,
            message = errors.FirstOrDefault()?.message ?? "Invalid request.",
            errors,
        });
    };
});

builder.Services.AddSupportDesk(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

await app.Services.InitializeInfrastructureAsync();

app.UseCors("frontend");

app.UseHttpsRedirection();

app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<CustomerDashboardHub>("hubs/customerDashboardHub");
app.MapHub<OrganizationDashboardHub>("hubs/organizationDashboardHub");
app.MapHub<TicketHub>("hubs/ticketHub");

app.Run();
