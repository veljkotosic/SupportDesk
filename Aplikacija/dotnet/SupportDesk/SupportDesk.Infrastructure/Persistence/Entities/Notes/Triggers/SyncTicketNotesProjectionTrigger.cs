using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Notes.Triggers;

public sealed class SyncTicketNotesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_notes_ticket_notes()
            RETURNS TRIGGER AS $$
            DECLARE
                v_author_name VARCHAR(256);
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    SELECT ""UserName"" INTO v_author_name FROM public.""Users"" WHERE ""Id"" = NEW.""AuthorId"";

                    INSERT INTO projections.ticket_notes (id, ticket_id, author_id, author_user_name, text, created_at)
                    VALUES (NEW.""Id"", NEW.""TicketId"", NEW.""AuthorId"", COALESCE(v_author_name, ''), NEW.""Text"", NEW.""CreatedAt"")
                    ON CONFLICT (id) DO NOTHING;

                    RETURN NEW;
                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.ticket_notes WHERE id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_notes_sync_ticket_notes ON public.""Notes"";
            CREATE TRIGGER trg_notes_sync_ticket_notes
            AFTER INSERT OR DELETE ON public.""Notes""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_notes_ticket_notes();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_notes_sync_ticket_notes ON public.""Notes"";
            DROP FUNCTION IF EXISTS projections.sync_notes_ticket_notes();
        ");
    }
}
