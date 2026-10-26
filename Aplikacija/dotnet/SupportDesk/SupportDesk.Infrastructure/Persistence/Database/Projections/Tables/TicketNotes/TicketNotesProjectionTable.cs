using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketNotes;

public sealed class TicketNotesProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.ticket_notes (
                id UUID PRIMARY KEY,
                ticket_id UUID NOT NULL,
                author_id UUID NOT NULL,
                author_user_name VARCHAR(256) NOT NULL,
                text TEXT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_proj_notes_ticket 
                ON projections.ticket_notes (ticket_id, created_at ASC);
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.ticket_notes CASCADE;
        ");
    }
}
