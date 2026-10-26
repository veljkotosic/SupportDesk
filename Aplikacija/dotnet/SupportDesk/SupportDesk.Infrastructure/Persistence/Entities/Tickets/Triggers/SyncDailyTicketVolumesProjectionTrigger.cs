using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets.Triggers;

public sealed class SyncDailyTicketVolumesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_tickets_daily_ticket_volumes()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    INSERT INTO projections.daily_ticket_volumes (organization_id, day, opened_count, closed_count)
                    VALUES (NEW.""OrganizationId"", NEW.""OpenedAt""::date, 1, 0)
                    ON CONFLICT (organization_id, day) DO UPDATE
                    SET opened_count = projections.daily_ticket_volumes.opened_count + 1;
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    IF NEW.""Status"" = 2 AND (OLD.""Status"" IS DISTINCT FROM 2) AND NEW.""ClosedAt"" IS NOT NULL THEN
                        INSERT INTO projections.daily_ticket_volumes (organization_id, day, opened_count, closed_count)
                        VALUES (NEW.""OrganizationId"", NEW.""ClosedAt""::date, 0, 1)
                        ON CONFLICT (organization_id, day) DO UPDATE
                        SET closed_count = projections.daily_ticket_volumes.closed_count + 1;
                    END IF;
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_tickets_sync_daily_ticket_volumes ON public.""Tickets"";
            CREATE TRIGGER trg_tickets_sync_daily_ticket_volumes
            AFTER INSERT OR UPDATE OF ""Status"", ""ClosedAt"" ON public.""Tickets""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_tickets_daily_ticket_volumes();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_tickets_sync_daily_ticket_volumes ON public.""Tickets"";
            DROP FUNCTION IF EXISTS projections.sync_tickets_daily_ticket_volumes();
        ");
    }
}
