using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Chat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InitiatorName",
                schema: "chat",
                table: "Chats",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetName",
                schema: "chat",
                table: "Chats",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitiatorName",
                schema: "chat",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "TargetName",
                schema: "chat",
                table: "Chats");
        }
    }
}
