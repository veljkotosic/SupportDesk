using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt",
                table: "Tickets",
                columns: new[] { "CustomerId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "CustomerId", "Status", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_Priority",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "Priority" });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_Priority",
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

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId",
                table: "Tickets",
                column: "CustomerId");

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
        }
    }
}
