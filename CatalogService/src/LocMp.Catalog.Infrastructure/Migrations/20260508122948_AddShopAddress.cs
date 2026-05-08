using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apartment",
                schema: "catalog",
                table: "Shops",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "catalog",
                table: "Shops",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Entrance",
                schema: "catalog",
                table: "Shops",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Floor",
                schema: "catalog",
                table: "Shops",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                schema: "catalog",
                table: "Shops",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "catalog",
                table: "Shops",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apartment",
                schema: "catalog",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "catalog",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Entrance",
                schema: "catalog",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Floor",
                schema: "catalog",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "HouseNumber",
                schema: "catalog",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Street",
                schema: "catalog",
                table: "Shops");
        }
    }
}
