using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.SupportAgents;

public sealed class SupportAgentsProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.support_agents (
                id UUID PRIMARY KEY,
                organization_id UUID NOT NULL,
                user_name VARCHAR(256) NOT NULL,
                email VARCHAR(256) NOT NULL,
                assigned_tickets INT NOT NULL DEFAULT 0,
                resolved_tickets INT NOT NULL DEFAULT 0,
                joined_at TIMESTAMPTZ NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_proj_agents_org 
                ON projections.support_agents (organization_id, user_name);
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.support_agents CASCADE;
        ");
    }
}
