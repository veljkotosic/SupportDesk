using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.TicketNotifications.Triggers;

public sealed class SyncTicketSummariesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_ticket_notifications_ticket_summaries()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    IF NEW.""Status"" = 0 THEN
                        UPDATE projections.ticket_summaries
                        SET unread_notifications_count = unread_notifications_count + 1
                        WHERE id = NEW.""TicketId"";
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    IF OLD.""Status"" = 0 AND NEW.""Status"" <> 0 THEN
                        UPDATE projections.ticket_summaries
                        SET unread_notifications_count = GREATEST(0, unread_notifications_count - 1)
                        WHERE id = NEW.""TicketId"";
                    ELSIF OLD.""Status"" <> 0 AND NEW.""Status"" = 0 THEN
                        UPDATE projections.ticket_summaries
                        SET unread_notifications_count = unread_notifications_count + 1
                        WHERE id = NEW.""TicketId"";
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    IF OLD.""Status"" = 0 THEN
                        UPDATE projections.ticket_summaries
                        SET unread_notifications_count = GREATEST(0, unread_notifications_count - 1)
                        WHERE id = OLD.""TicketId"";
                    END IF;
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_ticket_notifications_sync_ticket_summaries ON public.""TicketNotifications"";
            CREATE TRIGGER trg_ticket_notifications_sync_ticket_summaries
            AFTER INSERT OR DELETE OR UPDATE OF ""Status"" ON public.""TicketNotifications""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_ticket_notifications_ticket_summaries();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_ticket_notifications_sync_ticket_summaries ON public.""TicketNotifications"";
            DROP FUNCTION IF EXISTS projections.sync_ticket_notifications_ticket_summaries();
        ");
    }
}
