using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.OrganizationDashboards;

public sealed class OrganizationDashboardsBackfill : IProjectionBackfill
{
    public int Order => 3;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.organization_dashboards (organization_id, organization_name, open_tickets, assigned_tickets, closed_tickets, agent_count, last_updated_at)
            SELECT 
                o.""Id"", o.""Name"",
                COALESCE(COUNT(t.""Id"") FILTER (WHERE t.""Status"" = 0), 0)::int,
                COALESCE(COUNT(t.""Id"") FILTER (WHERE t.""Status"" = 1), 0)::int,
                COALESCE(COUNT(t.""Id"") FILTER (WHERE t.""Status"" = 2), 0)::int,
                COALESCE((SELECT COUNT(*) FROM public.""Users"" u WHERE u.""OrganizationId"" = o.""Id"" AND u.""Role"" = 1), 0)::int,
                NOW()
            FROM public.""Organizations"" o
            LEFT JOIN public.""Tickets"" t ON t.""OrganizationId"" = o.""Id""
            GROUP BY o.""Id"", o.""Name""
            ON CONFLICT (organization_id) DO UPDATE
            SET open_tickets = EXCLUDED.open_tickets,
                assigned_tickets = EXCLUDED.assigned_tickets,
                closed_tickets = EXCLUDED.closed_tickets,
                agent_count = EXCLUDED.agent_count;
        ");
    }
}
