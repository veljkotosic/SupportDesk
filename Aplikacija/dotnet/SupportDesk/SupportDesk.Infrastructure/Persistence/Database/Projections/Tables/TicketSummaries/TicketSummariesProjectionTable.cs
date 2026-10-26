using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportDesk.Infrastructure.Persistence.Database.Projections.Tables.TicketSummaries;

public sealed class TicketSummariesProjectionTable : IProjectionTable
{
    public void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS projections.ticket_summaries (
                id UUID PRIMARY KEY,
                organization_id UUID NOT NULL,
                organization_name VARCHAR(256) NOT NULL,
                category_id UUID NOT NULL,
                category_name VARCHAR(256) NOT NULL,
                customer_id UUID NOT NULL,
                customer_user_name VARCHAR(256) NOT NULL,
                customer_email VARCHAR(256) NOT NULL,
                support_agent_id UUID NULL,
                support_agent_user_name VARCHAR(256) NULL,
                status INT NOT NULL,
                priority INT NOT NULL,
                feedback VARCHAR(2048) NULL,
                subject VARCHAR(256) NOT NULL,
                opened_at TIMESTAMPTZ NOT NULL,
                assigned_at TIMESTAMPTZ NULL,
                closed_at TIMESTAMPTZ NULL,
                last_message_at TIMESTAMPTZ NOT NULL,
                unread_notifications_count INT NOT NULL DEFAULT 0
            );

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_customer_paged 
                ON projections.ticket_summaries (customer_id, last_message_at DESC NULLS LAST, id DESC);

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_org_latest 
                ON projections.ticket_summaries (organization_id, last_message_at DESC NULLS LAST, id DESC);

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_org_priority 
                ON projections.ticket_summaries (organization_id, priority DESC, last_message_at DESC NULLS LAST, id DESC);

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_org_oldest 
                ON projections.ticket_summaries (organization_id, last_message_at ASC NULLS LAST, id ASC);

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_org_status 
                ON projections.ticket_summaries (organization_id, status);

            CREATE INDEX IF NOT EXISTS idx_proj_tickets_customer_status 
                ON projections.ticket_summaries (customer_id, status);
        ");
    }

    public void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS projections.ticket_summaries CASCADE;
        ");
    }
}
