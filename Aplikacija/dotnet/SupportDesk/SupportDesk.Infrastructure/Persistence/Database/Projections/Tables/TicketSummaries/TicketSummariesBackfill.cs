using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketSummaries;

public sealed class TicketSummariesBackfill : IProjectionBackfill
{
    public int Order => 7;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.ticket_summaries (
                id, organization_id, organization_name, category_id, category_name,
                customer_id, customer_user_name, customer_email,
                support_agent_id, support_agent_user_name,
                status, priority, feedback, subject, opened_at, assigned_at, closed_at, last_message_at,
                unread_notifications_count
            )
            SELECT 
                t.""Id"", t.""OrganizationId"", o.""Name"", t.""CategoryId"", c.""Name"",
                t.""CustomerId"", cu.""UserName"", cu.""Email"",
                t.""SupportAgentId"", sa.""UserName"",
                t.""Status"", t.""Priority"", t.""Feedback"", t.""Subject"",
                t.""OpenedAt"", t.""AssignedAt"", t.""ClosedAt"", t.""LastMessageAt"",
                COALESCE(notifs.cnt, 0)::int
            FROM public.""Tickets"" t
            JOIN public.""Organizations"" o ON t.""OrganizationId"" = o.""Id""
            JOIN public.""Categories"" c ON t.""CategoryId"" = c.""Id""
            JOIN public.""Users"" cu ON t.""CustomerId"" = cu.""Id""
            LEFT JOIN public.""Users"" sa ON t.""SupportAgentId"" = sa.""Id""
            LEFT JOIN (
                SELECT ""TicketId"", COUNT(*) as cnt 
                FROM public.""TicketNotifications"" 
                WHERE ""Status"" = 0 
                GROUP BY ""TicketId""
            ) notifs ON notifs.""TicketId"" = t.""Id""
            ON CONFLICT (id) DO NOTHING;
        ");
    }
}
