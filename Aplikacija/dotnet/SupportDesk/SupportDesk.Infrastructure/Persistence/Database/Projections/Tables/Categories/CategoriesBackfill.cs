using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.CategoriesProjection;

public sealed class CategoriesBackfill : IProjectionBackfill
{
    public int Order => 1;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.categories (id, organization_id, name, description, ticket_count, deleted_at)
            SELECT c.""Id"", c.""OrganizationId"", c.""Name"", c.""Description"", COALESCE(COUNT(t.""Id""), 0)::int, c.""DeletedAt""
            FROM public.""Categories"" c
            LEFT JOIN public.""Tickets"" t ON t.""CategoryId"" = c.""Id""
            GROUP BY c.""Id"", c.""OrganizationId"", c.""Name"", c.""Description"", c.""DeletedAt""
            ON CONFLICT (id) DO UPDATE
            SET ticket_count = EXCLUDED.ticket_count, name = EXCLUDED.name, description = EXCLUDED.description;
        ");
    }
}
