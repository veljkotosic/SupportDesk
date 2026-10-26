using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Organizations.Triggers;

public sealed class SyncOrganizationDashboardsProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_organizations_organization_dashboards()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    INSERT INTO projections.organization_dashboards (organization_id, organization_name, open_tickets, assigned_tickets, closed_tickets, agent_count, last_updated_at)
                    VALUES (NEW.""Id"", NEW.""Name"", 0, 0, 0, 0, NOW())
                    ON CONFLICT (organization_id) DO NOTHING;
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    UPDATE projections.organization_dashboards
                    SET organization_name = NEW.""Name"", last_updated_at = NOW()
                    WHERE organization_id = NEW.""Id"";
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.organization_dashboards WHERE organization_id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_organizations_sync_organization_dashboards ON public.""Organizations"";
            CREATE TRIGGER trg_organizations_sync_organization_dashboards
            AFTER INSERT OR DELETE OR UPDATE OF ""Name"" ON public.""Organizations""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_organizations_organization_dashboards();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_organizations_sync_organization_dashboards ON public.""Organizations"";
            DROP FUNCTION IF EXISTS projections.sync_organizations_organization_dashboards();
        ");
    }
}
