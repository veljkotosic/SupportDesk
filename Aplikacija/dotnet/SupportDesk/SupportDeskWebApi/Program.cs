using DotNetEnv;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.DependencyInjection;
using SupportDeskWebApi.Hubs;

if (File.Exists("../../../.env"))
{
    Env.NoClobber().Load("../../../.env");
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<CustomerDashboardHub>("hubs/customerDashboardHub");
app.MapHub<OrganizationDashboardHub>("hubs/organizationDashboardHub");
app.MapHub<TicketHub>("hubs/ticketHub");

app.Run();
