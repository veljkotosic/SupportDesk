using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Auth.UserContext;

namespace SupportDesk.Infrastructure.Persistence.Database;

public class SupportDeskDbContextFactory : IDesignTimeDbContextFactory<SupportDeskDbContext>
{
    public SupportDeskDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SupportDeskDbContext>();
        
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=SupportDeskDatabase;Username=postgres;Password=postgres",
            npgsql => npgsql.MigrationsAssembly(typeof(SupportDeskDbContext).Assembly.FullName));

        return new SupportDeskDbContext(
            optionsBuilder.Options,
            new DesignTimeUserContext(),
            new DesignTimeTenantContext());
    }
    
    private sealed class DesignTimeUserContext : IUserContext
    {
        public Guid GetCurrentUserId() => Guid.Empty;
        public Guid? TryGetCurrentUserId() => null;
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? GetCurrentOrganizationId() => null;
    }
}