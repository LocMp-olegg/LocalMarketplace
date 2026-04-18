using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutGroupingToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CheckoutId",
                schema: "orders",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                schema: "orders",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                schema: "orders",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutId",
                schema: "orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopId",
                schema: "orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopName",
                schema: "orders",
                table: "Orders");
        }
    }
}
