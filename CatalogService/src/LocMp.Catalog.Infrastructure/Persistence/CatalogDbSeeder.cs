using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Infrastructure.Persistence;

public static class CatalogDbSeeder
{
    private static class CategoryIds
    {
        public static readonly Guid Food       = new("11111111-0000-0000-0000-000000000001");
        public static readonly Guid Bakery     = new("11111111-0000-0000-0000-000000000002");
        public static readonly Guid Vegetables = new("11111111-0000-0000-0000-000000000003");
        public static readonly Guid Handmade   = new("11111111-0000-0000-0000-000000000004");
        public static readonly Guid Jewelry    = new("11111111-0000-0000-0000-000000000005");
        public static readonly Guid Services   = new("11111111-0000-0000-0000-000000000006");
    }

    private static class TagIds
    {
        public static readonly Guid Homemade = new("22222222-0000-0000-0000-000000000001");
        public static readonly Guid Fresh    = new("22222222-0000-0000-0000-000000000002");
        public static readonly Guid Eco      = new("22222222-0000-0000-0000-000000000003");
        public static readonly Guid Handmade = new("22222222-0000-0000-0000-000000000004");
    }

    private static class SellerIds
    {
        public static readonly Guid Anna    = new("33333333-0000-0000-0000-000000000001");
        public static readonly Guid Mikhail = new("33333333-0000-0000-0000-000000000002");
        public static readonly Guid Olga    = new("33333333-0000-0000-0000-000000000003");
    }

    private static class ShopIds
    {
        public static readonly Guid AnnaBakery   = new("55555555-0000-0000-0000-000000000001");
        public static readonly Guid MikhailFarm  = new("55555555-0000-0000-0000-000000000002");
        public static readonly Guid OlgaHandmade = new("55555555-0000-0000-0000-000000000003");
    }

    private static class ProductIds
    {
        public static readonly Guid Bread    = new("44444444-0000-0000-0000-000000000001");
        public static readonly Guid Cake     = new("44444444-0000-0000-0000-000000000002");
        public static readonly Guid Tomatoes = new("44444444-0000-0000-0000-000000000003");
        public static readonly Guid Potatoes = new("44444444-0000-0000-0000-000000000004");
        public static readonly Guid EarRings = new("44444444-0000-0000-0000-000000000005");
        public static readonly Guid Necklace = new("44444444-0000-0000-0000-000000000006");
    }

    public static async Task SeedAsync(CatalogDbContext db)
    {
        if (await db.Categories.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        // ── Категории ────────────────────────────────────────────────────────

        db.Categories.AddRange(
            new(CategoryIds.Food)      { Name = "Еда и напитки",      SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Bakery)    { ParentCategoryId = CategoryIds.Food,     Name = "Выпечка и десерты", SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Vegetables){ ParentCategoryId = CategoryIds.Food,     Name = "Овощи и фрукты",    SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Handmade)  { Name = "Ручная работа",       SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Jewelry)   { ParentCategoryId = CategoryIds.Handmade, Name = "Украшения",         SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Services)  { Name = "Услуги",              SortOrder = 3, IsActive = true, CreatedAt = now }
        );

        // ── Теги ─────────────────────────────────────────────────────────────

        db.Tags.AddRange(
            new(TagIds.Homemade) { Name = "домашнее",      Slug = "homemade" },
            new(TagIds.Fresh)    { Name = "свежее",        Slug = "fresh" },
            new(TagIds.Eco)      { Name = "эко",           Slug = "eco" },
            new(TagIds.Handmade) { Name = "ручная работа", Slug = "handmade" }
        );

        // ── Продавцы (Read Models) ─────────────────────────────────────────────

        db.SellerReadModels.AddRange(
            new(SellerIds.Anna)    { DisplayName = "Анна К.",    AverageRating = 4.9m, ReviewCount = 37, Location = Pt(37.612, 55.758), LastSyncedAt = now },
            new(SellerIds.Mikhail) { DisplayName = "Михаил В.",  AverageRating = 4.6m, ReviewCount = 12, Location = Pt(37.635, 55.745), LastSyncedAt = now },
            new(SellerIds.Olga)   { DisplayName = "Ольга Д.",   AverageRating = 5.0m, ReviewCount = 8,  Location = Pt(37.598, 55.765), LastSyncedAt = now }
        );

        // ── Магазины ─────────────────────────────────────────────────────────

        db.Shops.AddRange(
            new(ShopIds.AnnaBakery)
            {
                SellerId = SellerIds.Anna, BusinessName = "Домашняя выпечка Анны",
                PhoneNumber = "+79001112233", Email = "anna@example.com",
                Description = "Домашняя выпечка и торты на заказ",
                BusinessType = BusinessType.Individual, WorkingHours = "Пн-Пт 7:00-12:00",
                ServiceRadiusMeters = 2000, IsActive = true, IsVerified = true,
                VerifiedAt = now, CreatedAt = now
            },
            new(ShopIds.MikhailFarm)
            {
                SellerId = SellerIds.Mikhail, BusinessName = "Фермерские овощи Михаила",
                PhoneNumber = "+79002223344", Email = "mikhail@example.com",
                Description = "Свежие овощи с грядки",
                BusinessType = BusinessType.Individual, WorkingHours = "Сб-Вс 9:00-14:00",
                ServiceRadiusMeters = 3000, IsActive = true, IsVerified = true,
                VerifiedAt = now, CreatedAt = now
            },
            new(ShopIds.OlgaHandmade)
            {
                SellerId = SellerIds.Olga, BusinessName = "Украшения ручной работы Ольги",
                PhoneNumber = "+79003334455", Email = "olga@example.com",
                Description = "Украшения из смолы и полимерной глины",
                BusinessType = BusinessType.Individual, WorkingHours = "Ежедневно 10:00-20:00",
                ServiceRadiusMeters = 5000, IsActive = true, IsVerified = true,
                VerifiedAt = now, CreatedAt = now
            }
        );

        // ── Товары ───────────────────────────────────────────────────────────

        db.Products.AddRange(
            new(ProductIds.Bread)
            {
                SellerId = SellerIds.Anna, ShopId = ShopIds.AnnaBakery, CategoryId = CategoryIds.Bakery,
                Name = "Ржаной хлеб на закваске",
                Description = "Домашний хлеб, выпекаю каждое утро. Без дрожжей и консервантов.",
                Price = 180, Unit = "шт", StockQuantity = 10, Location = Pt(37.612, 55.758), IsActive = true, CreatedAt = now
            },
            new(ProductIds.Cake)
            {
                SellerId = SellerIds.Anna, ShopId = ShopIds.AnnaBakery, CategoryId = CategoryIds.Bakery,
                Name = "Медовик домашний",
                Description = "Торт по бабушкиному рецепту. Вес 1.2 кг. Под заказ за сутки.",
                Price = 950, Unit = "шт", StockQuantity = 3, Location = Pt(37.612, 55.758), IsActive = true, CreatedAt = now
            },
            new(ProductIds.Tomatoes)
            {
                SellerId = SellerIds.Mikhail, ShopId = ShopIds.MikhailFarm, CategoryId = CategoryIds.Vegetables,
                Name = "Томаты черри с грядки",
                Description = "Выращены без химии на даче. Сезонные, только с куста.",
                Price = 120, Unit = "кг", StockQuantity = 15, Location = Pt(37.635, 55.745), IsActive = true, CreatedAt = now
            },
            new(ProductIds.Potatoes)
            {
                SellerId = SellerIds.Mikhail, ShopId = ShopIds.MikhailFarm, CategoryId = CategoryIds.Vegetables,
                Name = "Картофель молодой",
                Description = "Сорт Беллароза. Мешок 5 кг.",
                Price = 350, Unit = "шт", StockQuantity = 20, Location = Pt(37.635, 55.745), IsActive = true, CreatedAt = now
            },
            new(ProductIds.EarRings)
            {
                SellerId = SellerIds.Olga, ShopId = ShopIds.OlgaHandmade, CategoryId = CategoryIds.Jewelry,
                Name = "Серьги из эпоксидной смолы",
                Description = "Ручная работа. Внутри — сухоцветы. Диаметр 3 см.",
                Price = 650, Unit = "пара", StockQuantity = 5, Location = Pt(37.598, 55.765), IsActive = true, CreatedAt = now
            },
            new(ProductIds.Necklace)
            {
                SellerId = SellerIds.Olga, ShopId = ShopIds.OlgaHandmade, CategoryId = CategoryIds.Jewelry,
                Name = "Кулон «Лесная фея»",
                Description = "Из полимерной глины. Цепочка в комплекте. Длина 45 см.",
                Price = 890, Unit = "шт", StockQuantity = 2, Location = Pt(37.598, 55.765), IsActive = true, CreatedAt = now
            }
        );

        // ── Теги продуктов ────────────────────────────────────────────────────

        db.ProductTags.AddRange(
            new() { ProductId = ProductIds.Bread,    TagId = TagIds.Homemade },
            new() { ProductId = ProductIds.Bread,    TagId = TagIds.Fresh },
            new() { ProductId = ProductIds.Cake,     TagId = TagIds.Homemade },
            new() { ProductId = ProductIds.Tomatoes, TagId = TagIds.Fresh },
            new() { ProductId = ProductIds.Tomatoes, TagId = TagIds.Eco },
            new() { ProductId = ProductIds.Potatoes, TagId = TagIds.Eco },
            new() { ProductId = ProductIds.EarRings, TagId = TagIds.Handmade },
            new() { ProductId = ProductIds.Necklace, TagId = TagIds.Handmade }
        );

        // ── История остатков ──────────────────────────────────────────────────

        var productList = new[] {
            (ProductIds.Bread, 10), (ProductIds.Cake, 3), (ProductIds.Tomatoes, 15),
            (ProductIds.Potatoes, 20), (ProductIds.EarRings, 5), (ProductIds.Necklace, 2)
        };

        db.StockHistory.AddRange(productList.Select(p => new StockHistory(Guid.NewGuid())
        {
            ProductId = p.Item1,
            ChangeType = StockChangeType.InitialStock,
            QuantityDelta = p.Item2,
            QuantityAfter = p.Item2,
            CreatedAt = now
        }));

        await db.SaveChangesAsync();
    }

    private static Point Pt(double lon, double lat) => new(lon, lat) { SRID = 4326 };
}
