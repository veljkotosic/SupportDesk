using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections;

public interface IProjectionBackfill
{
    int Order => 0;
    void Backfill(MigrationBuilder migrationBuilder);
}
