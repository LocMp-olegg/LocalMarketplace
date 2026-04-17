using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Analytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.CreateTable(
                name: "DisputeSummaries",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    OpenedCount = table.Column<int>(type: "integer", nullable: false),
                    ResolvedCount = table.Column<int>(type: "integer", nullable: false),
                    BuyerFavoredCount = table.Column<int>(type: "integer", nullable: false),
                    SellerFavoredCount = table.Column<int>(type: "integer", nullable: false),
                    AverageResolutionMinutes = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeographicActivities",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplexId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplexName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    ActiveSellers = table.Column<int>(type: "integer", nullable: false),
                    ActiveBuyers = table.Column<int>(type: "integer", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeographicActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformDailySummaries",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    NewRegistrations = table.Column<int>(type: "integer", nullable: false),
                    ActiveBuyers = table.Column<int>(type: "integer", nullable: false),
                    ActiveSellers = table.Column<int>(type: "integer", nullable: false),
                    BlockedUsers = table.Column<int>(type: "integer", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    CompletedOrders = table.Column<int>(type: "integer", nullable: false),
                    CancelledOrders = table.Column<int>(type: "integer", nullable: false),
                    DisputedOrders = table.Column<int>(type: "integer", nullable: false),
                    GrossMerchandiseValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageOrderValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewProducts = table.Column<int>(type: "integer", nullable: false),
                    NewReviews = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformDailySummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductRatingSummaries",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRatingSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductViewCounters",
                schema: "analytics",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false),
                    ViewsToday = table.Column<int>(type: "integer", nullable: false),
                    ViewsThisWeek = table.Column<int>(type: "integer", nullable: false),
                    LastViewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductViewCounters", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "SellerLeaderboards",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderCount = table.Column<int>(type: "integer", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    RevenueRank = table.Column<int>(type: "integer", nullable: false),
                    OrderCountRank = table.Column<int>(type: "integer", nullable: false),
                    RanksComputedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerLeaderboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerRatingHistory",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    NewReviewsToday = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerRatingHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerSalesSummaries",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderCount = table.Column<int>(type: "integer", nullable: false),
                    AverageOrderValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CompletedCount = table.Column<int>(type: "integer", nullable: false),
                    CancelledCount = table.Column<int>(type: "integer", nullable: false),
                    DisputedCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerSalesSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockAlerts",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CurrentStock = table.Column<int>(type: "integer", nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopProducts",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TotalSold = table.Column<int>(type: "integer", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    FavoriteCount = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodType = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisputeSummaries_Date",
                schema: "analytics",
                table: "DisputeSummaries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeographicActivities_ComplexId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "GeographicActivities",
                columns: new[] { "ComplexId", "PeriodType", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDailySummaries_Date",
                schema: "analytics",
                table: "PlatformDailySummaries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatingSummaries_ProductId",
                schema: "analytics",
                table: "ProductRatingSummaries",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRatingSummaries_SellerId",
                schema: "analytics",
                table: "ProductRatingSummaries",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductViewCounters_SellerId",
                schema: "analytics",
                table: "ProductViewCounters",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerLeaderboards_PeriodType_PeriodStart_RevenueRank",
                schema: "analytics",
                table: "SellerLeaderboards",
                columns: new[] { "PeriodType", "PeriodStart", "RevenueRank" });

            migrationBuilder.CreateIndex(
                name: "IX_SellerLeaderboards_SellerId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerLeaderboards",
                columns: new[] { "SellerId", "PeriodType", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatingHistory_SellerId",
                schema: "analytics",
                table: "SellerRatingHistory",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerRatingHistory_SellerId_RecordedAt",
                schema: "analytics",
                table: "SellerRatingHistory",
                columns: new[] { "SellerId", "RecordedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SellerSalesSummaries_SellerId",
                schema: "analytics",
                table: "SellerSalesSummaries",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerSalesSummaries_SellerId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "SellerSalesSummaries",
                columns: new[] { "SellerId", "PeriodType", "PeriodStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_ProductId_AlertType",
                schema: "analytics",
                table: "StockAlerts",
                columns: new[] { "ProductId", "AlertType" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_SellerId",
                schema: "analytics",
                table: "StockAlerts",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAlerts_SellerId_IsAcknowledged",
                schema: "analytics",
                table: "StockAlerts",
                columns: new[] { "SellerId", "IsAcknowledged" });

            migrationBuilder.CreateIndex(
                name: "IX_TopProducts_SellerId",
                schema: "analytics",
                table: "TopProducts",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_TopProducts_SellerId_ProductId_PeriodType_PeriodStart",
                schema: "analytics",
                table: "TopProducts",
                columns: new[] { "SellerId", "ProductId", "PeriodType", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisputeSummaries",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "GeographicActivities",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "PlatformDailySummaries",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "ProductRatingSummaries",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "ProductViewCounters",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "SellerLeaderboards",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "SellerRatingHistory",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "SellerSalesSummaries",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "StockAlerts",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "TopProducts",
                schema: "analytics");
        }
    }
}
