using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.SupportAgents;

public sealed class SupportAgentsBackfill : IProjectionBackfill
{
    public int Order => 4;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.support_agents (id, organization_id, user_name, email, assigned_tickets, resolved_tickets, joined_at)
            SELECT u.""Id"", u.""OrganizationId"", u.""UserName"", u.""Email"",
                   COALESCE(COUNT(t.""Id"") FILTER (WHERE t.""Status"" = 2), 0)::int,
                   COALESCE(COUNT(t.""Id"") FILTER (WHERE t.""Status"" = 1), 0)::int,
                   u.""CreatedAt""
            FROM public.""Users"" u
            LEFT JOIN public.""Tickets"" t ON t.""SupportAgentId"" = u.""Id""
            WHERE u.""Role"" = 1 AND u.""OrganizationId"" IS NOT NULL
            GROUP BY u.""Id"", u.""OrganizationId"", u.""UserName"", u.""Email"", u.""CreatedAt""
            ON CONFLICT (id) DO UPDATE
            SET assigned_tickets = EXCLUDED.assigned_tickets, resolved_tickets = EXCLUDED.resolved_tickets;
        ");
    }
}
