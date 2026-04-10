using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductShopRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Shops_ShopId1",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ShopId1",
                schema: "catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShopId1",
                schema: "catalog",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShopId1",
                schema: "catalog",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShopId1",
                schema: "catalog",
                table: "Products",
                column: "ShopId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Shops_ShopId1",
                schema: "catalog",
                table: "Products",
                column: "ShopId1",
                principalSchema: "catalog",
                principalTable: "Shops",
                principalColumn: "Id");
        }
    }
}
