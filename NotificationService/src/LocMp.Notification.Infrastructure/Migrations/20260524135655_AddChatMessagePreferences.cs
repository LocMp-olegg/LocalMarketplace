using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessagePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChatMessages",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailChatMessages",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatMessages",
                schema: "notifs",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailChatMessages",
                schema: "notifs",
                table: "UserNotificationPreferences");
        }
    }
}
