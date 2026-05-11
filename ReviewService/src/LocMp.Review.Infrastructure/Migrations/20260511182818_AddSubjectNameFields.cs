using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Review.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubjectName",
                schema: "reviews",
                table: "Reviews",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductNames",
                schema: "reviews",
                table: "AllowedReviews",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                schema: "reviews",
                table: "AllowedReviews",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectName",
                schema: "reviews",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "ProductNames",
                schema: "reviews",
                table: "AllowedReviews");

            migrationBuilder.DropColumn(
                name: "SellerName",
                schema: "reviews",
                table: "AllowedReviews");
        }
    }
}
