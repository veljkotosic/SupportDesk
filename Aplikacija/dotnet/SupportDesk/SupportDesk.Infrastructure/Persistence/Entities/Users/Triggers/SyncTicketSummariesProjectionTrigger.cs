using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users.Triggers;

public sealed class SyncTicketSummariesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_users_ticket_summaries()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'UPDATE') THEN
                    IF NEW.""UserName"" IS DISTINCT FROM OLD.""UserName"" OR NEW.""Email"" IS DISTINCT FROM OLD.""Email"" THEN
                        UPDATE projections.ticket_summaries
                        SET customer_user_name = NEW.""UserName"", customer_email = NEW.""Email""
                        WHERE customer_id = NEW.""Id"";

                        UPDATE projections.ticket_summaries
                        SET support_agent_user_name = NEW.""UserName""
                        WHERE support_agent_id = NEW.""Id"";
                    END IF;
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_users_sync_ticket_summaries ON public.""Users"";
            CREATE TRIGGER trg_users_sync_ticket_summaries
            AFTER UPDATE OF ""UserName"", ""Email"" ON public.""Users""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_users_ticket_summaries();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_users_sync_ticket_summaries ON public.""Users"";
            DROP FUNCTION IF EXISTS projections.sync_users_ticket_summaries();
        ");
    }
}
