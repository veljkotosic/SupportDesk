using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets.Triggers;

public sealed class SyncOrganizationDashboardsProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_tickets_organization_dashboards()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    UPDATE projections.organization_dashboards
                    SET open_tickets = open_tickets + CASE WHEN NEW.""Status"" = 0 THEN 1 ELSE 0 END,
                        assigned_tickets = assigned_tickets + CASE WHEN NEW.""Status"" = 1 THEN 1 ELSE 0 END,
                        closed_tickets = closed_tickets + CASE WHEN NEW.""Status"" = 2 THEN 1 ELSE 0 END,
                        last_updated_at = NOW()
                    WHERE organization_id = NEW.""OrganizationId"";
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    IF NEW.""Status"" IS DISTINCT FROM OLD.""Status"" THEN
                        UPDATE projections.organization_dashboards
                        SET open_tickets = open_tickets + CASE WHEN NEW.""Status"" = 0 THEN 1 WHEN OLD.""Status"" = 0 THEN -1 ELSE 0 END,
                            assigned_tickets = assigned_tickets + CASE WHEN NEW.""Status"" = 1 THEN 1 WHEN OLD.""Status"" = 1 THEN -1 ELSE 0 END,
                            closed_tickets = closed_tickets + CASE WHEN NEW.""Status"" = 2 THEN 1 WHEN OLD.""Status"" = 2 THEN -1 ELSE 0 END,
                            last_updated_at = NOW()
                        WHERE organization_id = NEW.""OrganizationId"";
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    UPDATE projections.organization_dashboards
                    SET open_tickets = open_tickets - CASE WHEN OLD.""Status"" = 0 THEN 1 ELSE 0 END,
                        assigned_tickets = assigned_tickets - CASE WHEN OLD.""Status"" = 1 THEN 1 ELSE 0 END,
                        closed_tickets = closed_tickets - CASE WHEN OLD.""Status"" = 2 THEN 1 ELSE 0 END,
                        last_updated_at = NOW()
                    WHERE organization_id = OLD.""OrganizationId"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_tickets_sync_organization_dashboards ON public.""Tickets"";
            CREATE TRIGGER trg_tickets_sync_organization_dashboards
            AFTER INSERT OR DELETE OR UPDATE OF ""Status"" ON public.""Tickets""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_tickets_organization_dashboards();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_tickets_sync_organization_dashboards ON public.""Tickets"";
            DROP FUNCTION IF EXISTS projections.sync_tickets_organization_dashboards();
        ");
    }
}
