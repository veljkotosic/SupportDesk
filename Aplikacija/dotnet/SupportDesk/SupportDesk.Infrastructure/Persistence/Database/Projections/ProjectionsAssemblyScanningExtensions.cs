using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections;

public static class ProjectionsAssemblyScanningExtensions
{
    extension(MigrationBuilder migrationBuilder)
    {
        public void ApplyProjectionTablesFromAssembly(Assembly assembly)
        {
            var tables = assembly.GetTypes()
                .Where(t => typeof(IProjectionTable).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IProjectionTable)Activator.CreateInstance(t)!)
                .OrderBy(t => t.Order)
                .ThenBy(t => t.GetType().FullName)
                .ToList();

            foreach (var table in tables)
            {
                table.Up(migrationBuilder);
            }
        }

        public void RevertProjectionTablesFromAssembly(Assembly assembly)
        {
            var tables = assembly.GetTypes()
                .Where(t => typeof(IProjectionTable).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IProjectionTable)Activator.CreateInstance(t)!)
                .OrderByDescending(t => t.Order)
                .ThenByDescending(t => t.GetType().FullName)
                .ToList();

            foreach (var table in tables)
            {
                table.Down(migrationBuilder);
            }
        }

        public void ApplyEntityTriggersFromAssembly(Assembly assembly)
        {
            var triggers = assembly.GetTypes()
                .Where(t => typeof(IEntityTrigger).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IEntityTrigger)Activator.CreateInstance(t)!)
                .OrderBy(t => t.Order)
                .ThenBy(t => t.GetType().FullName)
                .ToList();

            foreach (var trigger in triggers)
            {
                trigger.Up(migrationBuilder);
            }
        }

        public void RevertEntityTriggersFromAssembly(Assembly assembly)
        {
            var triggers = assembly.GetTypes()
                .Where(t => typeof(IEntityTrigger).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IEntityTrigger)Activator.CreateInstance(t)!)
                .OrderByDescending(t => t.Order)
                .ThenByDescending(t => t.GetType().FullName)
                .ToList();

            foreach (var trigger in triggers)
            {
                trigger.Down(migrationBuilder);
            }
        }

        public void BackfillProjectionsFromAssembly(Assembly assembly)
        {
            var backfills = assembly.GetTypes()
                .Where(t => typeof(IProjectionBackfill).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IProjectionBackfill)Activator.CreateInstance(t)!)
                .OrderBy(t => t.Order)
                .ThenBy(t => t.GetType().FullName)
                .ToList();

            foreach (var backfill in backfills)
            {
                backfill.Backfill(migrationBuilder);
            }
        }
    }
}
