using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets.Triggers;

public sealed class SyncCategoriesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_tickets_categories()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    UPDATE projections.categories
                    SET ticket_count = ticket_count + 1
                    WHERE id = NEW.""CategoryId"";
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    UPDATE projections.categories
                    SET ticket_count = GREATEST(0, ticket_count - 1)
                    WHERE id = OLD.""CategoryId"";
                    RETURN OLD;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_tickets_sync_categories ON public.""Tickets"";
            CREATE TRIGGER trg_tickets_sync_categories
            AFTER INSERT OR DELETE ON public.""Tickets""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_tickets_categories();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_tickets_sync_categories ON public.""Tickets"";
            DROP FUNCTION IF EXISTS projections.sync_tickets_categories();
        ");
    }
}
