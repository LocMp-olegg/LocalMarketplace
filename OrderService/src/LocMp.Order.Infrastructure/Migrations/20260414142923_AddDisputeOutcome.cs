using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Outcome",
                schema: "orders",
                table: "Disputes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outcome",
                schema: "orders",
                table: "Disputes");
        }
    }
}
