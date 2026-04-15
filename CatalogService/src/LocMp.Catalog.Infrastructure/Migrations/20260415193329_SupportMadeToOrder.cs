using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportMadeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMadeToOrder",
                schema: "catalog",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                schema: "catalog",
                table: "Products",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMadeToOrder",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                schema: "catalog",
                table: "Products");
        }
    }
}
