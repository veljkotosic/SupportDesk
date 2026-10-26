using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketNotes;

public sealed class TicketNotesBackfill : IProjectionBackfill
{
    public int Order => 6;

    public void Backfill(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            INSERT INTO projections.ticket_notes (id, ticket_id, author_id, author_user_name, text, created_at)
            SELECT n.""Id"", n.""TicketId"", n.""AuthorId"", u.""UserName"", n.""Text"", n.""CreatedAt""
            FROM public.""Notes"" n
            JOIN public.""Users"" u ON n.""AuthorId"" = u.""Id""
            ON CONFLICT (id) DO NOTHING;
        ");
    }
}
