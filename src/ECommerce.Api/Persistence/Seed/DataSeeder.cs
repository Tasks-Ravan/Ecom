using ECommerce.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Persistence.Seed;

public static class DataSeeder
{
    private static readonly (string Name, string Slug, string? Parent, Guid Id)[] CategoryRows =
    [
        ("Electronics", "electronics", null, Guid.Parse("10000000-0000-0000-0000-000000000001")),
        ("Smartphones", "smartphones", "electronics", Guid.Parse("10000000-0000-0000-0000-000000000002")),
        ("Laptops", "laptops", "electronics", Guid.Parse("10000000-0000-0000-0000-000000000003")),
        ("Headphones", "headphones", "electronics", Guid.Parse("10000000-0000-0000-0000-000000000004")),
        ("Home Appliances", "home-appliances", null, Guid.Parse("10000000-0000-0000-0000-000000000005")),
        ("Kitchen Appliances", "kitchen-appliances", "home-appliances", Guid.Parse("10000000-0000-0000-0000-000000000006")),
        ("Vacuum Cleaners", "vacuum-cleaners", "home-appliances", Guid.Parse("10000000-0000-0000-0000-000000000007")),
        ("Fashion", "fashion", null, Guid.Parse("10000000-0000-0000-0000-000000000008")),
        ("Men's Clothing", "mens-clothing", "fashion", Guid.Parse("10000000-0000-0000-0000-000000000009")),
        ("Women's Clothing", "womens-clothing", "fashion", Guid.Parse("10000000-0000-0000-0000-00000000000a")),
        ("Shoes", "shoes", "fashion", Guid.Parse("10000000-0000-0000-0000-00000000000b")),
        ("Sports & Outdoor", "sports-outdoor", null, Guid.Parse("10000000-0000-0000-0000-00000000000c"))
    ];

    private static readonly (string Name, Guid Id)[] BrandRows =
    [
        ("Apple", Guid.Parse("20000000-0000-0000-0000-000000000001")),
        ("Samsung", Guid.Parse("20000000-0000-0000-0000-000000000002")),
        ("Xiaomi", Guid.Parse("20000000-0000-0000-0000-000000000003")),
        ("Sony", Guid.Parse("20000000-0000-0000-0000-000000000004")),
        ("Lenovo", Guid.Parse("20000000-0000-0000-0000-000000000005")),
        ("Philips", Guid.Parse("20000000-0000-0000-0000-000000000006")),
        ("Nike", Guid.Parse("20000000-0000-0000-0000-000000000007")),
        ("Adidas", Guid.Parse("20000000-0000-0000-0000-000000000008")),
        ("Zara", Guid.Parse("20000000-0000-0000-0000-000000000009")),
        ("Bosch", Guid.Parse("20000000-0000-0000-0000-00000000000a"))
    ];

    private sealed record ProductSeed(string Sku, string Category, string Brand, decimal Price, int Stock,
        string EnName, string EnDescription, string AzName, string AzDescription,
        string? RuName, string? RuDescription, bool Active = true);

    private static readonly ProductSeed[] ProductRows =
    [
        new("ELC-PHN-0001","smartphones","Apple",899,18,"iPhone 15","A powerful smartphone with a 48 MP camera.","iPhone 15","48 MP kameralı güclü smartfon.","iPhone 15","Мощный смартфон с камерой 48 Мп."),
        new("ELC-PHN-0002","smartphones","Samsung",799,22,"Galaxy S24","Flagship Android phone with a vivid display.","Galaxy S24","Canlı ekranlı flaqman Android telefonu.","Galaxy S24","Флагманский Android-смартфон с ярким экраном."),
        new("ELC-PHN-0003","smartphones","Xiaomi",279,35,"Redmi Note 13","Affordable phone with a large AMOLED display.","Redmi Note 13","Böyük AMOLED ekranlı sərfəli telefon.","Redmi Note 13","Доступный телефон с большим AMOLED-экраном."),
        new("ELC-PHN-0004","smartphones","Samsung",499,0,"Galaxy A55","Balanced mid-range phone with long battery life.","Galaxy A55","Uzunömürlü batareyası olan orta seqment telefon.","Galaxy A55","Сбалансированный смартфон среднего класса с ёмкой батареей."),
        new("ELC-LAP-0001","laptops","Apple",1099,9,"MacBook Air M2","Lightweight laptop with Apple M2 performance.","MacBook Air M2","Apple M2 prosessorlu yüngül noutbuk.","MacBook Air M2","Лёгкий ноутбук с производительностью Apple M2."),
        new("ELC-LAP-0002","laptops","Lenovo",749,14,"ThinkPad E14","Business laptop with a durable keyboard.","ThinkPad E14","Möhkəm klaviaturalı biznes noutbuku.","ThinkPad E14","Бизнес-ноутбук с надёжной клавиатурой."),
        new("ELC-LAP-0003","laptops","Lenovo",499,6,"IdeaPad Slim 3","Everyday laptop for study and home use.","IdeaPad Slim 3","Təhsil və gündəlik istifadə üçün noutbuk.","IdeaPad Slim 3","Ноутбук для учёбы и повседневных задач."),
        new("ELC-HDP-0001","headphones","Sony",349,12,"WH-1000XM5","Wireless noise-cancelling over-ear headphones.","WH-1000XM5","Səs-küyü azaldan simsiz qulaqüstü qulaqlıq.","WH-1000XM5","Беспроводные полноразмерные наушники с шумоподавлением."),
        new("ELC-HDP-0002","headphones","Apple",249,17,"AirPods Pro","Compact earbuds with adaptive noise cancellation.","AirPods Pro","Adaptiv səs-küy azaltmalı kompakt qulaqdaxili qulaqlıq.","AirPods Pro","Компактные вкладыши с адаптивным шумоподавлением."),
        new("ELC-HDP-0003","headphones","Samsung",99,30,"Galaxy Buds FE","Comfortable wireless earbuds for everyday listening.","Galaxy Buds FE","Gündəlik dinləmə üçün rahat simsiz qulaqlıqlar.","Galaxy Buds FE","Удобные беспроводные наушники на каждый день."),
        new("HOM-KIT-0001","kitchen-appliances","Philips",159,11,"Philips Airfryer XL","Large-capacity air fryer for crisp meals with less oil.","Philips Airfryer XL","Az yağla xırtıldayan yeməklər üçün geniş hava fritözü.","Philips Airfryer XL","Вместительная аэрогриль-фритюрница для блюд с меньшим количеством масла."),
        new("HOM-KIT-0002","kitchen-appliances","Bosch",79,24,"Bosch Blender","Multi-speed blender for smoothies and sauces.","Bosch Blender","Smuzilər və souslar üçün çoxsürətli blender.","Bosch Blender","Многоскоростной блендер для смузи и соусов."),
        new("HOM-KIT-0003","kitchen-appliances","Philips",39,4,"Philips Toaster","Two-slice toaster with adjustable browning.","Philips Toaster","Tənzimlənən qızartma rejimli iki dilimli toster.",null,null),
        new("HOM-VAC-0001","vacuum-cleaners","Bosch",229,7,"Bosch Vacuum Cleaner","Bagless vacuum cleaner for everyday home cleaning.","Bosch Vacuum Cleaner","Gündəlik ev təmizliyi üçün torbasız tozsoran.","Bosch Vacuum Cleaner","Безмешковый пылесос для ежедневной уборки дома."),
        new("HOM-VAC-0002","vacuum-cleaners","Philips",129,0,"Philips PowerPro","Compact vacuum with cyclonic suction technology.","Philips PowerPro","Siklon soruş texnologiyalı yığcam tozsoran.","Philips PowerPro","Компактный пылесос с циклонной системой всасывания.",false),
        new("FAS-SHO-0001","shoes","Nike",149,20,"Nike Air Max 270","Lightweight lifestyle trainers with visible Air cushioning.","Nike Air Max 270","Görünən Air yastıqlaması olan yüngül gündəlik idman ayaqqabısı.","Nike Air Max 270","Лёгкие повседневные кроссовки с видимой амортизацией Air."),
        new("FAS-SHO-0002","shoes","Adidas",179,16,"Adidas Ultraboost","Responsive running shoes with a soft knit upper.","Adidas Ultraboost","Yumşaq toxunma üst hissəli çevik qaçış ayaqqabısı.","Adidas Ultraboost","Беговые кроссовки с мягким вязаным верхом и отзывчивой подошвой."),
        new("FAS-SHO-0003","shoes","Nike",89,13,"Nike Revolution 6","Everyday running shoes with a cushioned feel.","Nike Revolution 6","Yumşaq altlıqlı gündəlik qaçış ayaqqabısı.",null,null),
        new("FAS-MEN-0001","mens-clothing","Zara",39,25,"Zara Men's Shirt","Cotton button-down shirt with a regular fit.","Zara kişi köynəyi","Adi biçimli pambıq düyməli köynək.","Мужская рубашка Zara","Хлопковая рубашка на пуговицах классического кроя."),
        new("FAS-MEN-0002","mens-clothing","Nike",34,19,"Nike Sports T-Shirt","Breathable training T-shirt for active days.","Nike idman köynəyi","Aktiv günlər üçün nəfəsalan məşq köynəyi.","Спортивная футболка Nike","Дышащая футболка для тренировок и активного отдыха."),
        new("FAS-WMN-0001","womens-clothing","Zara",69,12,"Zara Women's Dress","Flowing midi dress for day or evening wear.","Zara qadın donu","Gündəlik və axşam geyimi üçün zərif midi don.","Женское платье Zara","Свободное платье миди для дневного и вечернего образа."),
        new("FAS-WMN-0002","womens-clothing","Adidas",55,8,"Adidas Hoodie","Soft fleece hoodie for training and casual wear.","Adidas kapüşonlu sviter","Məşq və gündəlik geyim üçün yumşaq flis sviter.","Худи Adidas","Мягкое флисовое худи для тренировок и повседневной носки."),
        new("SPO-OUT-0001","sports-outdoor","Nike",29,40,"Nike Training Bottle","Reusable sports bottle with a secure screw cap.","Nike məşq su qabı","Möhkəm vintli qapağı olan təkrar istifadə edilən idman qabı.","Бутылка Nike для тренировок","Многоразовая спортивная бутылка с надёжной завинчивающейся крышкой."),
        new("SPO-OUT-0002","sports-outdoor","Adidas",45,10,"Adidas Training Bag","Durable duffel bag for gym essentials.","Adidas məşq çantası","İdman zalı ləvazimatları üçün davamlı çanta.",null,null),
        new("SPO-OUT-0003","sports-outdoor","Nike",119,3,"Nike Windrunner Jacket","Lightweight jacket for outdoor training.","Nike Windrunner gödəkçəsi","Açıq havada məşq üçün yüngül gödəkçə.","Куртка Nike Windrunner","Лёгкая куртка для тренировок на свежем воздухе.",false)
    ];

    public static async Task SeedAsync(IServiceProvider services, string customerPassword, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Categories.AnyAsync(cancellationToken) || await db.Brands.AnyAsync(cancellationToken) || await db.Products.AnyAsync(cancellationToken))
            return;

        var now = DateTimeOffset.UtcNow;
        var categories = CategoryRows.Select(row => new Category
        {
            Id = row.Id, Name = row.Name, Slug = ECommerce.Api.Common.SlugGenerator.Generate(row.Name),
            ParentCategoryId = row.Parent is null ? null : CategoryRows.Single(parent => parent.Slug == row.Parent).Id,
            IsActive = true, CreatedAt = now, UpdatedAt = now
        }).ToArray();
        var brands = BrandRows.Select(row => new Brand
        {
            Id = row.Id, Name = row.Name, Slug = ECommerce.Api.Common.SlugGenerator.Generate(row.Name),
            IsActive = true, CreatedAt = now, UpdatedAt = now
        }).ToArray();
        var categoryIds = CategoryRows.ToDictionary(row => row.Slug, row => row.Id);
        var brandIds = BrandRows.ToDictionary(row => row.Name, row => row.Id);
        var products = ProductRows.Select((row, index) => new Product
        {
            Id = Guid.Parse($"30000000-0000-0000-0000-{index + 1:000000000000}"), Sku = row.Sku,
            Price = row.Price, StockQuantity = row.Stock, CategoryId = categoryIds[row.Category], BrandId = brandIds[row.Brand],
            IsActive = row.Active, CreatedAt = now, UpdatedAt = now,
            Translations = CreateTranslations(row)
        }).ToArray();

        db.Categories.AddRange(categories);
        db.Brands.AddRange(brands);
        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(customerPassword))
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var customer = await userManager.FindByEmailAsync("customer@example.com");
            if (customer is null)
            {
                customer = new ApplicationUser
                {
                    Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), UserName = "customer@example.com",
                    Email = "customer@example.com", EmailConfirmed = true, FirstName = "Sample", LastName = "Customer"
                };
                var created = await userManager.CreateAsync(customer, customerPassword);
                if (!created.Succeeded)
                    throw new InvalidOperationException($"Could not seed sample customer: {string.Join("; ", created.Errors.Select(error => error.Description))}");
            }
            if (!await userManager.IsInRoleAsync(customer, UserRole.Customer))
            {
                var assigned = await userManager.AddToRoleAsync(customer, UserRole.Customer);
                if (!assigned.Succeeded)
                    throw new InvalidOperationException($"Could not assign sample customer role: {string.Join("; ", assigned.Errors.Select(error => error.Description))}");
            }
        }
    }

    private static Dictionary<string, ProductTranslation> CreateTranslations(ProductSeed row)
    {
        var translations = new Dictionary<string, ProductTranslation>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new(row.EnName, row.EnDescription),
            ["az"] = new(row.AzName, row.AzDescription)
        };
        if (row.RuName is not null) translations["ru"] = new ProductTranslation(row.RuName, row.RuDescription!);
        return translations;
    }
}
