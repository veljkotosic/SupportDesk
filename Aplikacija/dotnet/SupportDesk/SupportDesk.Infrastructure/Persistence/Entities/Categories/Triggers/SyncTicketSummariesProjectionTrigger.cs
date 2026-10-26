using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Categories.Triggers;

public sealed class SyncTicketSummariesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_categories_ticket_summaries()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'UPDATE') THEN
                    IF NEW.""Name"" IS DISTINCT FROM OLD.""Name"" THEN
                        UPDATE projections.ticket_summaries
                        SET category_name = NEW.""Name""
                        WHERE category_id = NEW.""Id"";
                    END IF;
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_categories_sync_ticket_summaries ON public.""Categories"";
            CREATE TRIGGER trg_categories_sync_ticket_summaries
            AFTER UPDATE ON public.""Categories""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_categories_ticket_summaries();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_categories_sync_ticket_summaries ON public.""Categories"";
            DROP FUNCTION IF EXISTS projections.sync_categories_ticket_summaries();
        ");
    }
}
