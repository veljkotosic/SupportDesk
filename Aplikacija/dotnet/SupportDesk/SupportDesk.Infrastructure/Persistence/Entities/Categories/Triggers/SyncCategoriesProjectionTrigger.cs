using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Categories.Triggers;

public sealed class SyncCategoriesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_categories_categories()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    INSERT INTO projections.categories (id, organization_id, name, description, ticket_count, deleted_at)
                    VALUES (NEW.""Id"", NEW.""OrganizationId"", NEW.""Name"", NEW.""Description"", 0, NEW.""DeletedAt"")
                    ON CONFLICT (id) DO UPDATE
                    SET name = EXCLUDED.name, description = EXCLUDED.description, deleted_at = EXCLUDED.deleted_at;
                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    UPDATE projections.categories
                    SET name = NEW.""Name"", description = NEW.""Description"", deleted_at = NEW.""DeletedAt""
                    WHERE id = NEW.""Id"";
                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.categories WHERE id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_categories_sync_categories ON public.""Categories"";
            CREATE TRIGGER trg_categories_sync_categories
            AFTER INSERT OR UPDATE OR DELETE ON public.""Categories""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_categories_categories();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_categories_sync_categories ON public.""Categories"";
            DROP FUNCTION IF EXISTS projections.sync_categories_categories();
        ");
    }
}
