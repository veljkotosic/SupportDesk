using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.DailyTicketVolumes;

public sealed class DailyTicketVolumesBackfill : IProjectionBackfill
{
    public int Order => 2;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.daily_ticket_volumes (organization_id, day, opened_count, closed_count)
            WITH date_grid AS (
                SELECT o.""Id"" AS org_id, d::date AS day
                FROM public.""Organizations"" o
                CROSS JOIN generate_series(CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE + INTERVAL '1 day', INTERVAL '1 day') d
            ),
            opened AS (
                SELECT ""OrganizationId"", ""OpenedAt""::date AS day, COUNT(*)::int AS cnt
                FROM public.""Tickets""
                GROUP BY ""OrganizationId"", ""OpenedAt""::date
            ),
            closed AS (
                SELECT ""OrganizationId"", ""ClosedAt""::date AS day, COUNT(*)::int AS cnt
                FROM public.""Tickets""
                WHERE ""Status"" = 1 AND ""ClosedAt"" IS NOT NULL
                GROUP BY ""OrganizationId"", ""ClosedAt""::date
            )
            SELECT 
                dg.org_id, dg.day,
                COALESCE(o.cnt, 0),
                COALESCE(c.cnt, 0)
            FROM date_grid dg
            LEFT JOIN opened o ON dg.org_id = o.""OrganizationId"" AND dg.day = o.day
            LEFT JOIN closed c ON dg.org_id = c.""OrganizationId"" AND dg.day = c.day
            WHERE COALESCE(o.cnt, 0) > 0 OR COALESCE(c.cnt, 0) > 0
            ON CONFLICT (organization_id, day) DO UPDATE
            SET opened_count = EXCLUDED.opened_count, closed_count = EXCLUDED.closed_count;
        ");
    }
}
