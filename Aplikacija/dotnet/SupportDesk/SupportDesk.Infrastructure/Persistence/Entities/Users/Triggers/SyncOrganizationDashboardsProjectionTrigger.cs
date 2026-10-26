using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users.Triggers;

public sealed class SyncOrganizationDashboardsProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_users_organization_dashboards()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    IF NEW.""Role"" = 1 AND NEW.""OrganizationId"" IS NOT NULL THEN
                        UPDATE projections.organization_dashboards
                        SET agent_count = agent_count + 1, last_updated_at = NOW()
                        WHERE organization_id = NEW.""OrganizationId"";
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    IF OLD.""Role"" = 1 AND OLD.""OrganizationId"" IS NOT NULL THEN
                        UPDATE projections.organization_dashboards
                        SET agent_count = GREATEST(0, agent_count - 1), last_updated_at = NOW()
                        WHERE organization_id = OLD.""OrganizationId"";
                    END IF;
                    RETURN OLD;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_users_sync_organization_dashboards ON public.""Users"";
            CREATE TRIGGER trg_users_sync_organization_dashboards
            AFTER INSERT OR DELETE ON public.""Users""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_users_organization_dashboards();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_users_sync_organization_dashboards ON public.""Users"";
            DROP FUNCTION IF EXISTS projections.sync_users_organization_dashboards();
        ");
    }
}
