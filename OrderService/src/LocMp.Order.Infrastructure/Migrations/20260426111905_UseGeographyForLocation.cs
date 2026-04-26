using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocMp.Order.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseGeographyForLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE orders."DeliveryAddresses"
                    ALTER COLUMN "Location" TYPE geography
                    USING "Location"::geography;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE orders."DeliveryAddresses"
                    ALTER COLUMN "Location" TYPE geometry(Point, 4326)
                    USING "Location"::geometry;
                """);
        }
    }
}
