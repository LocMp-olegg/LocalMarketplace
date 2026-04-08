using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMiniO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoData",
                schema: "media",
                table: "UserPhotos");

            migrationBuilder.AddColumn<string>(
                name: "ObjectKey",
                schema: "media",
                table: "UserPhotos",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageUrl",
                schema: "media",
                table: "UserPhotos",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectKey",
                schema: "media",
                table: "UserPhotos");

            migrationBuilder.DropColumn(
                name: "StorageUrl",
                schema: "media",
                table: "UserPhotos");

            migrationBuilder.AddColumn<byte[]>(
                name: "PhotoData",
                schema: "media",
                table: "UserPhotos",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
