using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxCourierDistanceToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxCourierDistanceMeters",
                schema: "catalog",
                table: "Shops",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxCourierDistanceMeters",
                schema: "catalog",
                table: "Shops");
        }
    }
}
