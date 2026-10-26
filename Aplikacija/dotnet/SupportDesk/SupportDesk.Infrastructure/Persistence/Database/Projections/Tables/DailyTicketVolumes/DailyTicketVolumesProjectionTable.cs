using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.DailyTicketVolumes;

public sealed class DailyTicketVolumesProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.daily_ticket_volumes (
                organization_id UUID NOT NULL,
                day DATE NOT NULL,
                opened_count INT NOT NULL DEFAULT 0,
                closed_count INT NOT NULL DEFAULT 0,
                PRIMARY KEY (organization_id, day)
            );
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.daily_ticket_volumes CASCADE;
        ");
    }
}
