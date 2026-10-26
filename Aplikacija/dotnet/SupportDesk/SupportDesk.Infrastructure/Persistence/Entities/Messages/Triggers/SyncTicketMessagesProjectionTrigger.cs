using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Messages.Triggers;

public sealed class SyncTicketMessagesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_messages_ticket_messages()
            RETURNS TRIGGER AS $$
            DECLARE
                v_sender_name VARCHAR(256);
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    SELECT ""UserName"" INTO v_sender_name FROM public.""Users"" WHERE ""Id"" = NEW.""SenderId"";

                    INSERT INTO projections.ticket_messages (id, ticket_id, sender_id, sender_user_name, text, created_at)
                    VALUES (NEW.""Id"", NEW.""TicketId"", NEW.""SenderId"", COALESCE(v_sender_name, ''), NEW.""Text"", NEW.""CreatedAt"")
                    ON CONFLICT (id) DO NOTHING;

                    RETURN NEW;
                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.ticket_messages WHERE id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_messages_sync_ticket_messages ON public.""Messages"";
            CREATE TRIGGER trg_messages_sync_ticket_messages
            AFTER INSERT OR DELETE ON public.""Messages""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_messages_ticket_messages();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_messages_sync_ticket_messages ON public.""Messages"";
            DROP FUNCTION IF EXISTS projections.sync_messages_ticket_messages();
        ");
    }
}
