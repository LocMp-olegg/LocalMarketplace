using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "ShopLocation",
                schema: "orders",
                table: "Orders",
                type: "geography",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourierApplications",
                schema: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourierId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CourierPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CourierLocation = table.Column<Point>(type: "geography", nullable: true),
                    DistanceToShopMeters = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourierApplications_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "orders",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourierApplications_CourierId",
                schema: "orders",
                table: "CourierApplications",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierApplications_OrderId",
                schema: "orders",
                table: "CourierApplications",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierApplications_OrderId_CourierId",
                schema: "orders",
                table: "CourierApplications",
                columns: new[] { "OrderId", "CourierId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierApplications",
                schema: "orders");

            migrationBuilder.DropColumn(
                name: "ShopLocation",
                schema: "orders",
                table: "Orders");
        }
    }
}
