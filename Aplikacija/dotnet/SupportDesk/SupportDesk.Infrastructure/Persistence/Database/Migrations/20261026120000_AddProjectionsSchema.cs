using Microsoft.EntityFrameworkCore.Migrations;
using SupportDesk.Infrastructure.Persistence.Database;
using SupportDesk.Infrastructure.Persistence.Database.Projections;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectionsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var infrastructureAssembly = typeof(SupportDeskDbContext).Assembly;

            // 1. Drop redundant composite query indexes on public tables
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt_Id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt_Id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_OpenedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_ClosedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_Priority",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportAgentId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportAgentId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketNotifications_OrganizationId_Status",
                table: "TicketNotifications");

            migrationBuilder.DropIndex(
                name: "IX_TicketNotifications_TicketId_Status",
                table: "TicketNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Notes_TicketId_CreatedAt",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TicketId_CreatedAt",
                table: "Messages");

            // 2. Create lean single-column FK indexes on public tables
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportAgentId",
                table: "Tickets",
                column: "SupportAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotifications_OrganizationId",
                table: "TicketNotifications",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotifications_TicketId",
                table: "TicketNotifications",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TicketId",
                table: "Notes",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TicketId",
                table: "Messages",
                column: "TicketId");

            // 3. Discover and create all projection schema and tables via assembly scanning
            migrationBuilder.ApplyProjectionTablesFromAssembly(infrastructureAssembly);

            // 4. Discover and register all entity triggers via assembly scanning
            migrationBuilder.ApplyEntityTriggersFromAssembly(infrastructureAssembly);

            // 5. Initial backfill from public schema to projections schema
            migrationBuilder.BackfillProjectionsFromAssembly(infrastructureAssembly);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var infrastructureAssembly = typeof(SupportDeskDbContext).Assembly;

            // 1. Drop all entity triggers via assembly scanning
            migrationBuilder.RevertEntityTriggersFromAssembly(infrastructureAssembly);

            // 2. Drop all projection tables and schema via assembly scanning
            migrationBuilder.RevertProjectionTablesFromAssembly(infrastructureAssembly);

            // 3. Revert clean FK indexes
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportAgentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketNotifications_OrganizationId",
                table: "TicketNotifications");

            migrationBuilder.DropIndex(
                name: "IX_TicketNotifications_TicketId",
                table: "TicketNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Notes_TicketId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TicketId",
                table: "Messages");

            // 4. Restore public composite query indexes
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt",
                table: "Tickets",
                columns: new[] { "CustomerId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt_Id",
                table: "Tickets",
                columns: new[] { "CustomerId", "LastMessageAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "CustomerId", "Status", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt_Id",
                table: "Tickets",
                columns: new[] { "OrganizationId", "LastMessageAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_OpenedAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "OpenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_ClosedAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "ClosedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "LastMessageAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_Priority",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportAgentId_Status",
                table: "Tickets",
                columns: new[] { "SupportAgentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportAgentId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "SupportAgentId", "Status", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotifications_OrganizationId_Status",
                table: "TicketNotifications",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotifications_TicketId_Status",
                table: "TicketNotifications",
                columns: new[] { "TicketId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_TicketId_CreatedAt",
                table: "Notes",
                columns: new[] { "TicketId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TicketId_CreatedAt",
                table: "Messages",
                columns: new[] { "TicketId", "CreatedAt" });
        }
    }
}
