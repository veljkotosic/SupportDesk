using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections;

public interface IProjectionTable
{
    int Order => 0;
    void Up(MigrationBuilder migrationBuilder);
    void Down(MigrationBuilder migrationBuilder);
}
