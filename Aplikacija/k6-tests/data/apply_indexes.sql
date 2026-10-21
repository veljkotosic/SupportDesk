--
-- Performance Indexes & Migrations (20261011000934, 20261020141926 & 20261021101051)
--
START TRANSACTION;

-- Drop legacy single-column indexes replaced by composite equivalents
DROP INDEX IF EXISTS public."IX_Tickets_OrganizationId";
DROP INDEX IF EXISTS public."IX_Tickets_SupportAgentId";
DROP INDEX IF EXISTS public."IX_Tickets_CustomerId";
DROP INDEX IF EXISTS public."IX_TicketNotifications_OrganizationId";
DROP INDEX IF EXISTS public."IX_TicketNotifications_TicketId";
DROP INDEX IF EXISTS public."IX_Notes_TicketId";
DROP INDEX IF EXISTS public."IX_Messages_TicketId";

-- Composite indexes for Ticket queries
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_LastMessageAt" ON public."Tickets" ("OrganizationId", "LastMessageAt" DESC NULLS LAST);
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_OpenedAt" ON public."Tickets" ("OrganizationId", "OpenedAt");
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_Status" ON public."Tickets" ("OrganizationId", "Status");
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_Status_ClosedAt" ON public."Tickets" ("OrganizationId", "Status", "ClosedAt");
CREATE INDEX IF NOT EXISTS "IX_Tickets_SupportAgentId_Status" ON public."Tickets" ("SupportAgentId", "Status");
CREATE INDEX IF NOT EXISTS "IX_Tickets_CustomerId_LastMessageAt" ON public."Tickets" ("CustomerId", "LastMessageAt");
CREATE INDEX IF NOT EXISTS "IX_Tickets_CustomerId_Status_LastMessageAt" ON public."Tickets" ("CustomerId", "Status", "LastMessageAt");
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_Status_LastMessageAt" ON public."Tickets" ("OrganizationId", "Status", "LastMessageAt" DESC NULLS LAST);
CREATE INDEX IF NOT EXISTS "IX_Tickets_OrganizationId_Status_Priority" ON public."Tickets" ("OrganizationId", "Status", "Priority");
CREATE INDEX IF NOT EXISTS "IX_Tickets_SupportAgentId_Status_LastMessageAt" ON public."Tickets" ("SupportAgentId", "Status", "LastMessageAt");

-- Composite indexes for TicketNotifications
CREATE INDEX IF NOT EXISTS "IX_TicketNotifications_OrganizationId_Status" ON public."TicketNotifications" ("OrganizationId", "Status");
CREATE INDEX IF NOT EXISTS "IX_TicketNotifications_TicketId_Status" ON public."TicketNotifications" ("TicketId", "Status");

-- Composite indexes for Notes and Messages
CREATE INDEX IF NOT EXISTS "IX_Notes_TicketId_CreatedAt" ON public."Notes" ("TicketId", "CreatedAt");
CREATE INDEX IF NOT EXISTS "IX_Messages_TicketId_CreatedAt" ON public."Messages" ("TicketId", "CreatedAt");

-- Register migrations in EF Core history table
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
    ('20261011000934_AddedTicketIndexesForDashboardQueries', '10.0.12'),
    ('20261020141926_AddQueryPerformanceIndexes', '10.0.12'),
    ('20261021101051_AddOptimizedDashboardIndexes', '10.0.12')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;
