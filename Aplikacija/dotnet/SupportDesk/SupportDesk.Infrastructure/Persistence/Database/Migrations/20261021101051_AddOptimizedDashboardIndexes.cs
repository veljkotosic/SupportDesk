using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOptimizedDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Tickets_OrganizationId_LastMessageAt\" ON \"Tickets\" (\"OrganizationId\", \"LastMessageAt\" DESC NULLS LAST);");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_ClosedAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "ClosedAt" });

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Tickets_OrganizationId_Status_LastMessageAt\" ON \"Tickets\" (\"OrganizationId\", \"Status\", \"LastMessageAt\" DESC NULLS LAST);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_ClosedAt",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_Status_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "Status", "LastMessageAt" });
        }
    }
}
