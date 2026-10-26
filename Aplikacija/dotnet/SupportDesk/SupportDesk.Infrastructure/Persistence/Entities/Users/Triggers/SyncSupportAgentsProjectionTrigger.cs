using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users.Triggers;

public sealed class SyncSupportAgentsProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_users_support_agents()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    IF NEW.""Role"" = 1 AND NEW.""OrganizationId"" IS NOT NULL THEN
                        INSERT INTO projections.support_agents (id, organization_id, user_name, email, assigned_tickets, resolved_tickets, joined_at)
                        VALUES (NEW.""Id"", NEW.""OrganizationId"", NEW.""UserName"", NEW.""Email"", 0, 0, NEW.""CreatedAt"")
                        ON CONFLICT (id) DO NOTHING;
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    IF NEW.""UserName"" IS DISTINCT FROM OLD.""UserName"" OR NEW.""Email"" IS DISTINCT FROM OLD.""Email"" THEN
                        UPDATE projections.support_agents
                        SET user_name = NEW.""UserName"", email = NEW.""Email""
                        WHERE id = NEW.""Id"";
                    END IF;
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.support_agents WHERE id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_users_sync_support_agents ON public.""Users"";
            CREATE TRIGGER trg_users_sync_support_agents
            AFTER INSERT OR UPDATE OR DELETE ON public.""Users""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_users_support_agents();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_users_sync_support_agents ON public.""Users"";
            DROP FUNCTION IF EXISTS projections.sync_users_support_agents();
        ");
    }
}
