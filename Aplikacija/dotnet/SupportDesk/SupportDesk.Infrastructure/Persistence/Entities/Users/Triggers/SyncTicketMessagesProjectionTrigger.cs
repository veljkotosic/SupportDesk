using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Users.Triggers;

public sealed class SyncTicketMessagesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_users_ticket_messages()
            RETURNS TRIGGER AS $$
            BEGIN
                IF (TG_OP = 'UPDATE') THEN
                    IF NEW.""UserName"" IS DISTINCT FROM OLD.""UserName"" THEN
                        UPDATE projections.ticket_messages
                        SET sender_user_name = NEW.""UserName""
                        WHERE sender_id = NEW.""Id"";
                    END IF;
                    RETURN NEW;
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_users_sync_ticket_messages ON public.""Users"";
            CREATE TRIGGER trg_users_sync_ticket_messages
            AFTER UPDATE ON public.""Users""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_users_ticket_messages();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_users_sync_ticket_messages ON public.""Users"";
            DROP FUNCTION IF EXISTS projections.sync_users_ticket_messages();
        ");
    }
}
