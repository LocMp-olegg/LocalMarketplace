using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddType4Dispute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisputeType",
                schema: "orders",
                table: "Disputes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisputeType",
                schema: "orders",
                table: "Disputes");
        }
    }
}
