using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.OrganizationDashboards;

public sealed class OrganizationDashboardsProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.organization_dashboards (
                organization_id UUID PRIMARY KEY,
                organization_name VARCHAR(256) NOT NULL,
                open_tickets INT NOT NULL DEFAULT 0,
                assigned_tickets INT NOT NULL DEFAULT 0,
                closed_tickets INT NOT NULL DEFAULT 0,
                agent_count INT NOT NULL DEFAULT 0,
                last_updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.organization_dashboards CASCADE;
        ");
    }
}
