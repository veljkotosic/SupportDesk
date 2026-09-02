using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class RevisedOutboxInboxPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandlerType",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<Guid>(
                name: "OutboxMessageId",
                table: "InboxMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutboxMessageId",
                table: "InboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "HandlerType",
                table: "OutboxMessages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
