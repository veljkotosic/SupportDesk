using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketMessages;

public sealed class TicketMessagesBackfill : IProjectionBackfill
{
    public int Order => 5;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.ticket_messages (id, ticket_id, sender_id, sender_user_name, text, created_at)
            SELECT m.""Id"", m.""TicketId"", m.""SenderId"", u.""UserName"", m.""Text"", m.""CreatedAt""
            FROM public.""Messages"" m
            JOIN public.""Users"" u ON m.""SenderId"" = u.""Id""
            ON CONFLICT (id) DO NOTHING;
        ");
    }
}
