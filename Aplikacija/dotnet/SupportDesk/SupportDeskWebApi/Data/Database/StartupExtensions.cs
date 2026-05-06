namespace SupportDeskWebApi.Data.Database;

public static class StartupExtensions
{
    extension(IServiceProvider services)
    {
        public async Task InitializeInfrastructureAsync()
        {
            using var scope = services.CreateScope();

            var serviceProvider = scope.ServiceProvider;

            await DatabaseInitializer.SeedRolesAsync(serviceProvider);
        }
    }
}