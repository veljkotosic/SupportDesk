using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets.Triggers;

public sealed class SyncSupportAgentsProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_tickets_support_agents()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'UPDATE') THEN
                    IF NEW.""SupportAgentId"" IS NOT NULL THEN
                        UPDATE projections.support_agents
                        SET assigned_tickets = assigned_tickets + CASE WHEN NEW.""Status"" = 1 AND OLD.""Status"" <> 1 THEN 1 WHEN NEW.""Status"" <> 1 AND OLD.""Status"" = 1 THEN -1 ELSE 0 END,
                            resolved_tickets = resolved_tickets + CASE WHEN NEW.""Status"" = 2 AND OLD.""Status"" <> 2 THEN 1 WHEN NEW.""Status"" <> 2 AND OLD.""Status"" = 2 THEN -1 ELSE 0 END
                        WHERE id = NEW.""SupportAgentId"";
                    END IF;
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_tickets_sync_support_agents ON public.""Tickets"";
            CREATE TRIGGER trg_tickets_sync_support_agents
            AFTER UPDATE ON public.""Tickets""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_tickets_support_agents();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_tickets_sync_support_agents ON public.""Tickets"";
            DROP FUNCTION IF EXISTS projections.sync_tickets_support_agents();
        ");
    }
}
