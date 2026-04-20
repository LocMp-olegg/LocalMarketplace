using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailEnabled",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailOrderUpdates",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailReviewReplies",
                schema: "notifs",
                table: "UserNotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "notifs",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailEnabled",
                schema: "notifs",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailOrderUpdates",
                schema: "notifs",
                table: "UserNotificationPreferences");

            migrationBuilder.DropColumn(
                name: "EmailReviewReplies",
                schema: "notifs",
                table: "UserNotificationPreferences");
        }
    }
}
