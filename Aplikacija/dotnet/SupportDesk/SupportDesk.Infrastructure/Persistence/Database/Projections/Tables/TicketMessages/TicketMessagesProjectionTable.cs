using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketMessages;

public sealed class TicketMessagesProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.ticket_messages (
                id UUID PRIMARY KEY,
                ticket_id UUID NOT NULL,
                sender_id UUID NOT NULL,
                sender_user_name VARCHAR(256) NOT NULL,
                text TEXT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_proj_messages_ticket 
                ON projections.ticket_messages (ticket_id, created_at ASC);
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.ticket_messages CASCADE;
        ");
    }
}
