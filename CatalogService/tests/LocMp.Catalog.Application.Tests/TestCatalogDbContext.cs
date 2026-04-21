using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LocMp.Catalog.Application.Tests;

/// <summary>
/// InMemory-safe DbContext: replaces Npgsql-specific column types (jsonb, geometry)
/// with value converters so EF InMemory can store them without a real PostgreSQL instance.
/// </summary>
internal sealed class TestCatalogDbContext(DbContextOptions<CatalogDbContext> options)
    : CatalogDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // JsonDocument → string for InMemory
        modelBuilder.Entity<Product>()
            .Property(p => p.Attributes)
            .HasConversion(
                v => v == null ? null : v.RootElement.GetRawText(),
                v => v == null ? null : JsonDocument.Parse(v));

        // NetTopologySuite Point → WKT string for InMemory
        modelBuilder.Entity<Product>()
            .Property(p => p.Location)
            .HasConversion(
                v => v == null ? null : $"{v.X},{v.Y}",
                v => v == null ? null : new NetTopologySuite.Geometries.Point(
                    double.Parse(v.Split(',')[0]),
                    double.Parse(v.Split(',')[1])) { SRID = 4326 });
    }
}
