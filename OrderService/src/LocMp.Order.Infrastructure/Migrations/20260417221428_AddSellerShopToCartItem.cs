using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerShopToCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                schema: "orders",
                table: "CartItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SellerName",
                schema: "orders",
                table: "CartItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                schema: "orders",
                table: "CartItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                schema: "orders",
                table: "CartItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellerId",
                schema: "orders",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SellerName",
                schema: "orders",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ShopId",
                schema: "orders",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ShopName",
                schema: "orders",
                table: "CartItems");
        }
    }
}
