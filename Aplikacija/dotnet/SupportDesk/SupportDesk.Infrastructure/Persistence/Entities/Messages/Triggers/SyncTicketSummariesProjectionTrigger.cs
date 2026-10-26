using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Messages.Triggers;

public sealed class SyncTicketSummariesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_messages_ticket_summaries()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    UPDATE projections.ticket_summaries
                    SET last_message_at = NEW.""CreatedAt""
                    WHERE id = NEW.""TicketId"";
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_messages_sync_ticket_summaries ON public.""Messages"";
            CREATE TRIGGER trg_messages_sync_ticket_summaries
            AFTER INSERT ON public.""Messages""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_messages_ticket_summaries();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_messages_sync_ticket_summaries ON public.""Messages"";
            DROP FUNCTION IF EXISTS projections.sync_messages_ticket_summaries();
        ");
    }
}
