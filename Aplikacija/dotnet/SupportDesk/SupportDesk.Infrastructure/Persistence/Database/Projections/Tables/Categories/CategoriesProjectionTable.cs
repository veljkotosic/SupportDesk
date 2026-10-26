using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.CategoriesProjection;

public sealed class CategoriesProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.categories (
                id UUID PRIMARY KEY,
                organization_id UUID NOT NULL,
                name VARCHAR(256) NOT NULL,
                description VARCHAR(1024) NULL,
                ticket_count INT NOT NULL DEFAULT 0,
                deleted_at TIMESTAMPTZ NULL
            );

            CREATE INDEX IF NOT EXISTS idx_proj_categories_org 
                ON projections.categories (organization_id, name) 
                WHERE deleted_at IS NULL;
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.categories CASCADE;
        ");
    }
}
