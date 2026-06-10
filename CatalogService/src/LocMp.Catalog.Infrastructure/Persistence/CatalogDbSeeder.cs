using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Infrastructure.Persistence;

public static class CatalogDbSeeder
{
    private static class CategoryIds
    {
        // ── Корневые разделы ───────────────────────────────────────────────────
        public static readonly Guid Food          = new("11111111-0000-0000-0000-000000000001");
        public static readonly Guid Handmade      = new("11111111-0000-0000-0000-000000000004");
        public static readonly Guid PlantsFlowers = new("11111111-0000-0000-0000-000000000017");
        public static readonly Guid Services      = new("11111111-0000-0000-0000-000000000006");
        public static readonly Guid Electronics   = new("11111111-0000-0000-0000-000000000022");
        public static readonly Guid Books         = new("11111111-0000-0000-0000-000000000023");

        // ── Еда и напитки ──────────────────────────────────────────────────────
        public static readonly Guid Bakery        = new("11111111-0000-0000-0000-000000000002"); // 'выпечк' → Croissant
        public static readonly Guid BreadRolls    = new("11111111-0000-0000-0000-000000000009"); // 'хлеб' → Wheat
        public static readonly Guid Desserts      = new("11111111-0000-0000-0000-000000000010"); // 'десерт' → CakeSlice
        public static readonly Guid Vegetables    = new("11111111-0000-0000-0000-000000000003"); // 'овощ' → Leaf
        public static readonly Guid Fruits        = new("11111111-0000-0000-0000-000000000011"); // 'фрукт' → Apple
        public static readonly Guid Berries       = new("11111111-0000-0000-0000-000000000012"); // 'ягод' → Cherry
        public static readonly Guid Meat          = new("11111111-0000-0000-0000-000000000013"); // 'мясо' → Beef
        public static readonly Guid Fish          = new("11111111-0000-0000-0000-000000000014"); // 'рыба' → Fish
        public static readonly Guid Dairy         = new("11111111-0000-0000-0000-000000000015"); // 'молоко'/'сыр' → Milk
        public static readonly Guid Drinks        = new("11111111-0000-0000-0000-000000000016"); // 'напит' → Coffee
        public static readonly Guid Preserves     = new("11111111-0000-0000-0000-000000000007");
        public static readonly Guid Spices        = new("11111111-0000-0000-0000-000000000008");

        // ── Ручная работа ──────────────────────────────────────────────────────
        public static readonly Guid Jewelry       = new("11111111-0000-0000-0000-000000000005"); // 'украшени' → Gem
        public static readonly Guid Clothes       = new("11111111-0000-0000-0000-000000000020"); // 'одежд' → Shirt
        public static readonly Guid Accessories   = new("11111111-0000-0000-0000-000000000021"); // 'аксессуар' → ShoppingBag

        // ── Растения и цветы ───────────────────────────────────────────────────
        public static readonly Guid IndoorPlants  = new("11111111-0000-0000-0000-000000000018"); // 'растени' → Leaf
        public static readonly Guid Flowers       = new("11111111-0000-0000-0000-000000000019"); // 'цветы'/'цветок' → Flower2
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

    // ════════════════════════════════════════════════════════════════════════

    public static async Task SeedAsync(CatalogDbContext db)
    {
        var now = DateTimeOffset.UtcNow;

        // Phase 1: чистая установка — всё сразу (23 категории + все данные)
        if (!await db.Categories.AnyAsync())
        {
            SeedAllCategories(db, now);
            SeedTagsAndData(db, now);
            await db.SaveChangesAsync();
            return;
        }

        // Phase 1.5: существующая БД — добавляем только новые категории
        if (!await db.Categories.AnyAsync(c => c.Id == CategoryIds.Meat))
        {
            SeedExtendedCategories(db, now);
            await db.SaveChangesAsync();
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Все 23 категории (для чистой установки)
    // ════════════════════════════════════════════════════════════════════════

    private static void SeedAllCategories(CatalogDbContext db, DateTimeOffset now)
    {
        db.Categories.AddRange(
            // ── Корневые ─────────────────────────────────────────────────────
            new(CategoryIds.Food)          { Name = "Еда и напитки",          SortOrder = 1,  IsActive = true, CreatedAt = now },
            new(CategoryIds.Handmade)      { Name = "Ручная работа",           SortOrder = 2,  IsActive = true, CreatedAt = now },
            new(CategoryIds.PlantsFlowers) { Name = "Растения и цветы",        SortOrder = 3,  IsActive = true, CreatedAt = now },
            new(CategoryIds.Services)      { Name = "Услуги",                  SortOrder = 4,  IsActive = true, CreatedAt = now },
            new(CategoryIds.Electronics)   { Name = "Электроника",             SortOrder = 5,  IsActive = true, CreatedAt = now },
            new(CategoryIds.Books)         { Name = "Книги и медиа",           SortOrder = 6,  IsActive = true, CreatedAt = now },

            // ── Еда и напитки → подкатегории ─────────────────────────────────
            new(CategoryIds.Bakery)     { ParentCategoryId = CategoryIds.Food, Name = "Выпечка и десерты",   SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.BreadRolls) { ParentCategoryId = CategoryIds.Bakery, Name = "Хлеб и булочки",   SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Desserts)   { ParentCategoryId = CategoryIds.Bakery, Name = "Десерты и торты",   SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Vegetables) { ParentCategoryId = CategoryIds.Food, Name = "Овощи",               SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Fruits)     { ParentCategoryId = CategoryIds.Vegetables, Name = "Фрукты",        SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Berries)    { ParentCategoryId = CategoryIds.Vegetables, Name = "Ягоды",         SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Meat)       { ParentCategoryId = CategoryIds.Food, Name = "Мясо и птица",        SortOrder = 3, IsActive = true, CreatedAt = now },
            new(CategoryIds.Fish)       { ParentCategoryId = CategoryIds.Food, Name = "Рыба и морепродукты", SortOrder = 4, IsActive = true, CreatedAt = now },
            new(CategoryIds.Dairy)      { ParentCategoryId = CategoryIds.Food, Name = "Молочные продукты",   SortOrder = 5, IsActive = true, CreatedAt = now },
            new(CategoryIds.Drinks)     { ParentCategoryId = CategoryIds.Food, Name = "Напитки",             SortOrder = 6, IsActive = true, CreatedAt = now },
            new(CategoryIds.Preserves)  { ParentCategoryId = CategoryIds.Food, Name = "Консервы и заготовки",SortOrder = 7, IsActive = true, CreatedAt = now },
            new(CategoryIds.Spices)     { ParentCategoryId = CategoryIds.Food, Name = "Специи и травы",      SortOrder = 8, IsActive = true, CreatedAt = now },

            // ── Ручная работа → подкатегории ─────────────────────────────────
            new(CategoryIds.Jewelry)     { ParentCategoryId = CategoryIds.Handmade, Name = "Украшения",      SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Clothes)     { ParentCategoryId = CategoryIds.Handmade, Name = "Одежда",         SortOrder = 2, IsActive = true, CreatedAt = now },
            new(CategoryIds.Accessories) { ParentCategoryId = CategoryIds.Handmade, Name = "Аксессуары",     SortOrder = 3, IsActive = true, CreatedAt = now },

            // ── Растения и цветы → подкатегории ──────────────────────────────
            new(CategoryIds.IndoorPlants) { ParentCategoryId = CategoryIds.PlantsFlowers, Name = "Комнатные растения", SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Flowers)      { ParentCategoryId = CategoryIds.PlantsFlowers, Name = "Цветы и букеты",     SortOrder = 2, IsActive = true, CreatedAt = now }
        );
    }

    // ════════════════════════════════════════════════════════════════════════
    // Новые категории для существующей БД (добавляются к старым 6)
    // ════════════════════════════════════════════════════════════════════════

    private static void SeedExtendedCategories(CatalogDbContext db, DateTimeOffset now)
    {
        db.Categories.AddRange(
            // Новые корневые
            new(CategoryIds.PlantsFlowers) { Name = "Растения и цветы", SortOrder = 30, IsActive = true, CreatedAt = now },
            new(CategoryIds.Electronics)   { Name = "Электроника",      SortOrder = 50, IsActive = true, CreatedAt = now },
            new(CategoryIds.Books)         { Name = "Книги и медиа",    SortOrder = 60, IsActive = true, CreatedAt = now },

            // Подкатегории Выпечки (существующий Bakery 002)
            new(CategoryIds.BreadRolls) { ParentCategoryId = CategoryIds.Bakery, Name = "Хлеб и булочки", SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Desserts)   { ParentCategoryId = CategoryIds.Bakery, Name = "Десерты и торты", SortOrder = 2, IsActive = true, CreatedAt = now },

            // Подкатегории Овощей (существующий Vegetables 003)
            new(CategoryIds.Fruits)  { ParentCategoryId = CategoryIds.Vegetables, Name = "Фрукты", SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Berries) { ParentCategoryId = CategoryIds.Vegetables, Name = "Ягоды",  SortOrder = 2, IsActive = true, CreatedAt = now },

            // Новые в Еде (существующий Food 001)
            new(CategoryIds.Meat)     { ParentCategoryId = CategoryIds.Food, Name = "Мясо и птица",        SortOrder = 30, IsActive = true, CreatedAt = now },
            new(CategoryIds.Fish)     { ParentCategoryId = CategoryIds.Food, Name = "Рыба и морепродукты", SortOrder = 40, IsActive = true, CreatedAt = now },
            new(CategoryIds.Dairy)    { ParentCategoryId = CategoryIds.Food, Name = "Молочные продукты",   SortOrder = 50, IsActive = true, CreatedAt = now },
            new(CategoryIds.Drinks)   { ParentCategoryId = CategoryIds.Food, Name = "Напитки",             SortOrder = 60, IsActive = true, CreatedAt = now },
            new(CategoryIds.Preserves){ ParentCategoryId = CategoryIds.Food, Name = "Консервы и заготовки",SortOrder = 70, IsActive = true, CreatedAt = now },
            new(CategoryIds.Spices)   { ParentCategoryId = CategoryIds.Food, Name = "Специи и травы",      SortOrder = 80, IsActive = true, CreatedAt = now },

            // Новые в Ручной работе (существующий Handmade 004)
            new(CategoryIds.Clothes)     { ParentCategoryId = CategoryIds.Handmade, Name = "Одежда",     SortOrder = 20, IsActive = true, CreatedAt = now },
            new(CategoryIds.Accessories) { ParentCategoryId = CategoryIds.Handmade, Name = "Аксессуары", SortOrder = 30, IsActive = true, CreatedAt = now },

            // Подкатегории нового раздела Растения и цветы
            new(CategoryIds.IndoorPlants) { ParentCategoryId = CategoryIds.PlantsFlowers, Name = "Комнатные растения", SortOrder = 1, IsActive = true, CreatedAt = now },
            new(CategoryIds.Flowers)      { ParentCategoryId = CategoryIds.PlantsFlowers, Name = "Цветы и букеты",     SortOrder = 2, IsActive = true, CreatedAt = now }
        );
    }

    // ════════════════════════════════════════════════════════════════════════
    // Теги, продавцы, магазины, товары — без изменений
    // ════════════════════════════════════════════════════════════════════════

    private static void SeedTagsAndData(CatalogDbContext db, DateTimeOffset now)
    {
        db.Tags.AddRange(
            new(TagIds.Homemade) { Name = "домашнее",      Slug = "домашнее" },
            new(TagIds.Fresh)    { Name = "свежее",        Slug = "свежее" },
            new(TagIds.Eco)      { Name = "эко",           Slug = "эко" },
            new(TagIds.Handmade) { Name = "ручная работа", Slug = "ручная-работа" }
        );

        db.SellerReadModels.AddRange(
            new(SellerIds.Anna)    { DisplayName = "Анна К.",   AverageRating = 4.9m, ReviewCount = 37, Location = Pt(37.612, 55.758), LastSyncedAt = now },
            new(SellerIds.Mikhail) { DisplayName = "Михаил В.", AverageRating = 4.6m, ReviewCount = 12, Location = Pt(37.635, 55.745), LastSyncedAt = now },
            new(SellerIds.Olga)    { DisplayName = "Ольга Д.",  AverageRating = 5.0m, ReviewCount = 8,  Location = Pt(37.598, 55.765), LastSyncedAt = now }
        );

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

        var productList = new[] {
            (ProductIds.Bread, 10), (ProductIds.Cake, 3), (ProductIds.Tomatoes, 15),
            (ProductIds.Potatoes, 20), (ProductIds.EarRings, 5), (ProductIds.Necklace, 2)
        };

        db.StockHistory.AddRange(productList.Select(p => new StockHistory(Guid.NewGuid())
        {
            ProductId     = p.Item1,
            ChangeType    = StockChangeType.InitialStock,
            QuantityDelta = p.Item2,
            QuantityAfter = p.Item2,
            CreatedAt     = now
        }));
    }

    private static Point Pt(double lon, double lat) => new(lon, lat) { SRID = 4326 };
}
