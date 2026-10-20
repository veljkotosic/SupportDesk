using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedTicketIndexesForDashboardQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportAgentId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_OpenedAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "OpenedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportAgentId_Status",
                table: "Tickets",
                columns: new[] { "SupportAgentId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_OpenedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportAgentId_Status",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId",
                table: "Tickets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportAgentId",
                table: "Tickets",
                column: "SupportAgentId");
        }
    }
}
