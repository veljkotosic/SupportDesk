using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

namespace SupportDesk.Infrastructure.Persistence.Entities.Tickets.Triggers;

public sealed class SyncTicketSummariesProjectionTrigger : IEntityTrigger
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION projections.sync_tickets_ticket_summaries()
            RETURNS TRIGGER AS $$
            DECLARE
                v_org_name VARCHAR(256);
                v_cat_name VARCHAR(256);
                v_cust_name VARCHAR(256);
                v_cust_email VARCHAR(256);
                v_agent_name VARCHAR(256);
            BEGIN
                IF (TG_OP = 'INSERT') THEN
                    SELECT ""Name"" INTO v_org_name FROM public.""Organizations"" WHERE ""Id"" = NEW.""OrganizationId"";
                    SELECT ""Name"" INTO v_cat_name FROM public.""Categories"" WHERE ""Id"" = NEW.""CategoryId"";
                    SELECT ""UserName"", ""Email"" INTO v_cust_name, v_cust_email FROM public.""Users"" WHERE ""Id"" = NEW.""CustomerId"";
                    IF NEW.""SupportAgentId"" IS NOT NULL THEN
                        SELECT ""UserName"" INTO v_agent_name FROM public.""Users"" WHERE ""Id"" = NEW.""SupportAgentId"";
                    END IF;

                    INSERT INTO projections.ticket_summaries (
                        id, organization_id, organization_name, category_id, category_name,
                        customer_id, customer_user_name, customer_email,
                        support_agent_id, support_agent_user_name,
                        status, priority, feedback, subject, opened_at, assigned_at, closed_at, last_message_at
                    ) VALUES (
                        NEW.""Id"", NEW.""OrganizationId"", COALESCE(v_org_name, ''), NEW.""CategoryId"", COALESCE(v_cat_name, ''),
                        NEW.""CustomerId"", COALESCE(v_cust_name, ''), COALESCE(v_cust_email, ''),
                        NEW.""SupportAgentId"", v_agent_name,
                        NEW.""Status"", NEW.""Priority"", NEW.""Feedback"", NEW.""Subject"",
                        NEW.""OpenedAt"", NEW.""AssignedAt"", NEW.""ClosedAt"", NEW.""LastMessageAt""
                    ) ON CONFLICT (id) DO UPDATE
                    SET status = EXCLUDED.status, priority = EXCLUDED.priority, feedback = EXCLUDED.feedback,
                        subject = EXCLUDED.subject, assigned_at = EXCLUDED.assigned_at, closed_at = EXCLUDED.closed_at,
                        last_message_at = EXCLUDED.last_message_at, support_agent_id = EXCLUDED.support_agent_id,
                        support_agent_user_name = EXCLUDED.support_agent_user_name;

                    RETURN NEW;

                ELSIF (TG_OP = 'UPDATE') THEN
                    IF NEW.""SupportAgentId"" IS DISTINCT FROM OLD.""SupportAgentId"" THEN
                        IF NEW.""SupportAgentId"" IS NOT NULL THEN
                            SELECT ""UserName"" INTO v_agent_name FROM public.""Users"" WHERE ""Id"" = NEW.""SupportAgentId"";
                        ELSE
                            v_agent_name := NULL;
                        END IF;

                        UPDATE projections.ticket_summaries
                        SET status = NEW.""Status"",
                            priority = NEW.""Priority"",
                            feedback = NEW.""Feedback"",
                            subject = NEW.""Subject"",
                            assigned_at = NEW.""AssignedAt"",
                            closed_at = NEW.""ClosedAt"",
                            last_message_at = NEW.""LastMessageAt"",
                            support_agent_id = NEW.""SupportAgentId"",
                            support_agent_user_name = v_agent_name
                        WHERE id = NEW.""Id"";
                    ELSE
                        UPDATE projections.ticket_summaries
                        SET status = NEW.""Status"",
                            priority = NEW.""Priority"",
                            feedback = NEW.""Feedback"",
                            subject = NEW.""Subject"",
                            assigned_at = NEW.""AssignedAt"",
                            closed_at = NEW.""ClosedAt"",
                            last_message_at = NEW.""LastMessageAt""
                        WHERE id = NEW.""Id"";
                    END IF;

                    RETURN NEW;

                ELSIF (TG_OP = 'DELETE') THEN
                    DELETE FROM projections.ticket_summaries WHERE id = OLD.""Id"";
                    RETURN OLD;
                END IF;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_tickets_sync_ticket_summaries ON public.""Tickets"";
            CREATE TRIGGER trg_tickets_sync_ticket_summaries
            AFTER INSERT OR UPDATE OR DELETE ON public.""Tickets""
            FOR EACH ROW EXECUTE FUNCTION projections.sync_tickets_ticket_summaries();
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TRIGGER IF EXISTS trg_tickets_sync_ticket_summaries ON public.""Tickets"";
            DROP FUNCTION IF EXISTS projections.sync_tickets_ticket_summaries();
        ");
    }
}
