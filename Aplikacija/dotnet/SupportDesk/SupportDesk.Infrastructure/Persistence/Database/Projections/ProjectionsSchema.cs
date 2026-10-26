using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections;

public sealed class ProjectionsSchema : IProjectionTable
{
    public int Order => -100;

    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE SCHEMA IF NOT EXISTS projections;
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP SCHEMA IF EXISTS projections CASCADE;
        ");
    }
}
