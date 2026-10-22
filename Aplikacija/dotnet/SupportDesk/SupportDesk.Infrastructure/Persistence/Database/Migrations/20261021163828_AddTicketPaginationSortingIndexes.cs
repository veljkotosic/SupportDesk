using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPaginationSortingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt_Id",
                table: "Tickets",
                columns: new[] { "CustomerId", "LastMessageAt", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt_Id",
                table: "Tickets",
                columns: new[] { "OrganizationId", "LastMessageAt", "Id" },
                descending: new[] { false, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_CustomerId_LastMessageAt_Id",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt_Id",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OrganizationId_LastMessageAt",
                table: "Tickets",
                columns: new[] { "OrganizationId", "LastMessageAt" },
                descending: new[] { false, true });
        }
    }
}
