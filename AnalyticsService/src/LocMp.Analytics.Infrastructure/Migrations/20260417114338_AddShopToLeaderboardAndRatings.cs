using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopToLeaderboardAndRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerLeaderboards_SellerId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerLeaderboards");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                schema: "analytics",
                table: "SellerLeaderboards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                schema: "analytics",
                table: "SellerLeaderboards",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                schema: "analytics",
                table: "ProductRatingSummaries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopName",
                schema: "analytics",
                table: "ProductRatingSummaries",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerLeaderboards_SellerId_ShopId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerLeaderboards",
                columns: new[] { "SellerId", "ShopId", "PeriodType", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SellerLeaderboards_SellerId_ShopId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerLeaderboards");

            migrationBuilder.DropColumn(
                name: "ShopId",
                schema: "analytics",
                table: "SellerLeaderboards");

            migrationBuilder.DropColumn(
                name: "ShopName",
                schema: "analytics",
                table: "SellerLeaderboards");

            migrationBuilder.DropColumn(
                name: "ShopId",
                schema: "analytics",
                table: "ProductRatingSummaries");

            migrationBuilder.DropColumn(
                name: "ShopName",
                schema: "analytics",
                table: "ProductRatingSummaries");

            migrationBuilder.CreateIndex(
                name: "IX_SellerLeaderboards_SellerId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerLeaderboards",
                columns: new[] { "SellerId", "PeriodType", "PeriodStart" },
                unique: true);
        }
    }
}
