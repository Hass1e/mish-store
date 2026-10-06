using Microsoft.EntityFrameworkCore;
using Mish.Api.Models;
using System.Security.Cryptography;

namespace Mish.Api.Data;

public static class DbSeeder
{
    public static async Task SeedProductsAsync(AppDbContext context)
    {
        if (!await context.Products.AnyAsync())
        {
            var products = new[]
            {
               new Product
            {
                Id = 1,
                Name = "Футболка белая, стандартная",
                Price = 1199,
                OldPrice = 1399,
                Description = "Стильная и комфортная мужская/женская футболка из мягкого дышащего хлопка. Эта модель отлично подходит для повседневной носки, спорта или прогулок. Свободный крой не сковывает движения, а качественный материал сохраняет форму и цвет даже после множества стирок. Футболка легко сочетается с джинсами, шортами и спортивными брюками.",
                ImageUrls = new List<string> { "/uploads/T-shirt/T-shirt1.png", "/uploads/T-shirt/T-shirt12.png" },
                SKU = "",
                CategoryId = 2,
                Composition = "Хлопок 100%",
                Weight = 200.0m,
                Stock = 993,
                Bought = 7,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.Parse("2026-10-01 20:43:39.643071")
            },
            new Product
            {
                Id = 2,
                Name = "Футболка",
                Price = 1299,
                OldPrice = 1111,
                Description = "крутая",
                ImageUrls = new List<string> { "/uploads/T-shirt/T-shirt2.png" },
                SKU = "",
                CategoryId = 0,
                Composition = "",
                Weight = 0,
                Stock = 396,
                Bought = 4,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.MinValue
            },
            new Product
            {
                Id = 3,
                Name = "Цветные футболки сезон Осень",
                Price = 655.0m,
                OldPrice = 899.0m,
                Description = "ага",
                ImageUrls = new List<string> 
                { 
                    "/uploads/T-shirt/T-shirt3.png", 
                    "/uploads/e29ccd2e-5e87-4876-9e9d-2d94d9b1e8e8.jpg", 
                    "/uploads/51652b75-3b8d-468f-a139-ca7db1df94ea.jpg", 
                    "/uploads/6207c5cd-ff74-4953-bf65-f0f98c6c3f56.jpg" 
                },
                SKU = "",
                CategoryId = 2,
                Composition = "",
                Weight = 0,
                Stock = 293,
                Bought = 7,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.Parse("2026-10-02 21:12:20.138538")
            },
            new Product
            {
                Id = 4,
                Name = "Футболка",
                Price = 1199,
                OldPrice = 1299,
                Description = "крутая",
                ImageUrls = new List<string>(),
                SKU = "",
                CategoryId = 0,
                Composition = "",
                Weight = 0,
                Stock = 200,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.MinValue
            },
            new Product
            {
                Id = 5,
                Name = "Футболка CJ G",
                Price = 1000,
                OldPrice = 2990.0m,
                Description = "Это универсальный и стильный элемент базового гардероба, который отлично сочетает в себе комфорт и современный уличный стиль. Насыщенный зеленый оттенок освежает повседневные образы и легко комбинируется как с классическим хуи, так и со спортивной одеждой.",
                ImageUrls = new List<string> { "/uploads/T-shirt/T-shirt11.png" },
                SKU = "",
                CategoryId = 2,
                Composition = "",
                Weight = 200.0m,
                Stock = 600,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.Parse("2026-10-01 18:14:11.465284")
            },
            new Product
            {
                Id = 6,
                Name = "Тестовая футболка",
                Price = 1990.0m,
                OldPrice = 2490.0m,
                Description = "Очень крутая тестовая футболка",
                ImageUrls = new List<string> { "/uploads/T-shirt/T-shirt1.png" },
                SKU = "MISH-75BDC728",
                CategoryId = 2,
                Composition = "100% хлопок",
                Weight = 200,
                Stock = 100,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = true,
                IsHit = false,
                Tags = new List<string> { "новинка", "хлопок" },
                CreatedAt = DateTime.Parse("2026-09-13 22:48:46.782527"),
                UpdatedAt = DateTime.Parse("2026-10-01 22:46:16.957998")
            },
            new Product
            {
                Id = 7,
                Name = "футболка зеленная",
                Price = 999.0m,
                OldPrice = 1100.0m,
                Description = "Дело не в том, что это просто вещь. Это самый недооценённый хак для твоего гардероба. Пока все вокруг покупают сотый по счёту чёрный оверсайз или банальный белый базовый хлопок, ты забираешь цвет, который транслирует уверенность, свежесть и стиль без лишних усилий.",
                ImageUrls = new List<string> { "/uploads/3256fc52-ca57-4e4b-bc5f-09a0e692d4de.jpg" },
                SKU = "MISH-3C3B4E96",
                CategoryId = 0,
                Composition = "хлоп",
                Weight = 100.0m,
                Stock = 1213,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-09-16 21:35:55.368175"),
                UpdatedAt = DateTime.Parse("2026-09-29 14:11:22.806049")
            },
            new Product
            {
                Id = 8,
                Name = "Кеды Летние",
                Price = 2220.0m,
                OldPrice = null,
                Description = "Ультралегкие городские кеды с анатомической стелькой и перфорацией. Созданы для долгих прогулок и путешествий. Сетчатый верх обеспечивает максимальную циркуляцию воздуха, а упругая подошва снимает нагрузку с суставов. Лаконичный силуэт дополнен прочными шнурками. Эта пара идеально держит форму и дарит ощущение легкости с утра до вечера.",
                ImageUrls = new List<string> 
                { 
                    "/uploads/16e724e7-82a9-42a8-88de-8174e6519eb0.jpg", 
                    "/uploads/376d9d72-b992-41e9-a6ce-107272b6113f.jpg" 
                },
                SKU = "MISH-5CE3F0CF",
                CategoryId = 0,
                Composition = "",
                Weight = 210.0m,
                Stock = 12,
                Bought = 1,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-09-18 18:48:18.148509"),
                UpdatedAt = DateTime.Parse("2026-09-26 15:16:01.466689")
            },
            new Product
            {
                Id = 9,
                Name = "Летние Кеды",
                Price = 3000.0m,
                OldPrice = null,
                Description = "Летние кеды, удобные в передвижение в городе",
                ImageUrls = new List<string> 
                { 
                    "/uploads/fc48741c-ce42-4b10-8c65-b1650a0cdee1.jpg", 
                    "/uploads/45fc2cc6-a83e-40f6-a446-e83b40445153.jpg", 
                    "/uploads/a52fb96f-a6b2-48d2-aeec-b95c4a727664.jpg" 
                },
                SKU = "MISH-096D2B0E",
                CategoryId = 0,
                Composition = "Резина, хлопок, полиэстер",
                Weight = 600.0m,
                Stock = 98,
                Bought = 2,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = true,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-09-19 01:05:37.985436"),
                UpdatedAt = DateTime.Parse("2026-09-19 01:05:37.985449")
            },
            new Product
            {
                Id = 10,
                Name = "Кепка летняя",
                Price = 444.0m,
                OldPrice = null,
                Description = "Летняя кепка — это не просто защита от солнца, а стильный элемент вашего повседневного образа.Мы создали идеальный головной убор для жарких дней, объединив максимальный комфорт и современный уличный стиль. Эта кепка отлично подойдет для прогулок по городу, отдыха на пляже, занятий спортом на свежем воздухе и путешествий.",
                ImageUrls = new List<string> { "/uploads/63a9feea-7592-4088-b446-0820e103ba9e.jpg" },
                SKU = "MISH-085B76B6",
                CategoryId = 2004,
                Composition = "",
                Weight = 150.0m,
                Stock = 100,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-09-20 21:51:36.635722"),
                UpdatedAt = DateTime.Parse("2026-10-01 18:28:04.56079")
            },
            new Product
            {
                Id = 11,
                Name = "Зимние кросовки",
                Price = 4000.0m,
                OldPrice = null,
                Description = "Зимние кеды — это стильная и теплая обувь, которая объединяет свободу городского стиля и надежную защиту от холода.\nГлавные особенности\n• Теплая подкладка: Густой мех или натуральная шерсть греют ноги в морозы.\n• Прочный верх: Плотная кожа или замша со специальной пропиткой защищают от влаги и снега.\n• Толстая подошва: Резиновая подошва с глубоким протектором не скользит на льду и сохраняет тепло.",
                ImageUrls = new List<string> { "/uploads/0478f369-6c09-4d57-9ba5-5656ce1d273b.jpg" },
                SKU = "MISH-2F35ADCF",
                CategoryId = 1002,
                Composition = "",
                Weight = 350.0m,
                Stock = 11,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-10-01 20:40:40.850892"),
                UpdatedAt = DateTime.Parse("2026-10-01 20:44:07.051736")
            },
            new Product
            {
                Id = 12,
                Name = "Комплект курьера",
                Price = 2000.0m,
                OldPrice = null,
                Description = "Необходимая экипировка для работы курьерской службы известной компании.",
                ImageUrls = new List<string> 
                { 
                    "/uploads/41812be4-d641-467f-acf4-6b3f42415f6c.jpg", 
                    "/uploads/2c8c89c0-af8a-4dd6-93ad-282891d3ed11.jpg", 
                    "/uploads/68b5bca6-5ca0-4c96-bf8f-e99bbfc948ff.jpg" 
                },
                SKU = "MISH-FDFD39B9",
                CategoryId = 0,
                Composition = "Хлопок",
                Weight = 600.0m,
                Stock = 2,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-10-01 21:28:40.196032"),
                UpdatedAt = DateTime.Parse("2026-10-01 21:29:10.757887")
            },
            new Product
            {
                Id = 13,
                Name = "Штаны бронзовые",
                Price = 2000.0m,
                OldPrice = 2999.0m,
                Description = "В модной индустрии эти штаны, выполненные из ткани бронзового оттенка и с металлизированным напылением (эффект «жидкого металла»).Бронзовые штаны — это экстравагантный, смелый и футуристичный элемент гардероба, который сочетает в себе благородный металлический блеск, дух ретро-футуризма и непреклонную прочность.",
                ImageUrls = new List<string> { "/uploads/a05c6a11-a3fb-44ea-9089-40d6056d545a.jpg" },
                SKU = "MISH-B04002DF",
                CategoryId = 6,
                Composition = "",
                Weight = 500.0m,
                Stock = 0,
                Bought = 0,
                Rating = 0,
                Reviews = 0,
                IsActive = true,
                IsNew = false,
                IsHit = false,
                Tags = new List<string>(),
                CreatedAt = DateTime.Parse("2026-10-02 11:50:50.92692"),
                UpdatedAt = DateTime.Parse("2026-10-02 11:51:47.884852")
            }
            };

            foreach (var product in products)
            {
                product.CreatedAt = AsUtc(product.CreatedAt);
                product.UpdatedAt = AsUtc(product.UpdatedAt);
            }

            context.Products.AddRange(products);
        }

        var legacyImageMappings = new[]
        {
            (Sku: "TSH-001", OldUrl: "/images/tshirt.jpg", NewUrl: "/uploads/T-shirt/T-shirt1.png"),
            (Sku: "HOO-001", OldUrl: "/images/hoodie.jpg", NewUrl: "/uploads/placeholder.jpg"),
            (Sku: "CAP-001", OldUrl: "/images/cap.jpg", NewUrl: "/uploads/placeholder.jpg")
        };

        foreach (var (sku, oldUrl, newUrl) in legacyImageMappings)
        {
            var product = await context.Products
                .SingleOrDefaultAsync(item => item.SKU == sku);
            if (product?.ImageUrls.Contains(oldUrl) == true)
            {
                product.ImageUrls = product.ImageUrls
                    .Select(url => url == oldUrl ? newUrl : url)
                    .ToList();
            }
        }

        await context.SaveChangesAsync();
        await SynchronizeIdentitySequenceAsync(context, "Products");
    }

    public static async Task SeedAdminUserAsync(AppDbContext context, string? initialPassword)
    {
        const int adminId = 5;
        const string username = "admin";
        var createdAt = new DateTime(2026, 9, 24, 21, 41, 21, DateTimeKind.Utc)
            .AddTicks(8288410);

        var admin = await context.Users
            .SingleOrDefaultAsync(user => user.Username.ToLower() == username);

        if (admin is null)
        {
            if (await context.Users.AnyAsync(user => user.Id == adminId))
            {
                throw new InvalidOperationException(
                    $"Cannot seed admin user: user ID {adminId} is already assigned to another account.");
            }

            admin = new User { Id = adminId };
            context.Users.Add(admin);
        }

        if (!IsBcryptHash(admin.PasswordHash))
        {
            if (string.IsNullOrEmpty(initialPassword))
            {
                initialPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
                Console.WriteLine(
                    $"Initial admin account created. Username: {username}; temporary password: {initialPassword}");
                Console.WriteLine("Save this password now; it will not be shown again.");
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword, workFactor: 12);
        }

        admin.Username = username;
        admin.Role = "Admin";
        admin.CreatedAt = createdAt;

        await context.SaveChangesAsync();
        await SynchronizeIdentitySequenceAsync(context, "Users");
    }

    public static async Task SeedPermissionsAsync(AppDbContext context)
    {
        var permissions = new (string Name, string Description)[]
        {
            ("orders.view_own", "Просмотр своих заказов"),
            ("orders.view_all", "Просмотр всех заказов"),
            ("orders.update_status", "Изменение статуса заказа"),
            ("products.create", "Создание товаров"),
            ("products.edit_own", "Редактирование своих товаров"),
            ("products.edit_any", "Редактирование любых товаров"),
            ("products.delete", "Удаление товаров"),
            ("users.manage_roles", "Управление ролями пользователей"),
            ("orders.force_status_change", "Принудительное изменение статуса заказа")
        };

        foreach (var (name, description) in permissions)
        {
            var permission = await context.Permissions
                .SingleOrDefaultAsync(item => item.Name == name);
            if (permission is null)
            {
                context.Permissions.Add(new Permission
                {
                    Name = name,
                    Description = description
                });
            }
            else
            {
                permission.Description = description;
            }
        }

        await context.SaveChangesAsync();

        var roles = new (string Name, string Description)[]
        {
            ("User", "Покупатель"),
            ("Seller", "Продавец"),
            ("Packer", "Упаковщик/Транспортник"),
            ("Support", "Техподдержка"),
            ("Moderator", "Модератор"),
            ("Admin", "Администратор")
        };

        foreach (var (name, description) in roles)
        {
            var role = await context.Roles.SingleOrDefaultAsync(item => item.Name == name);
            if (role is null)
            {
                context.Roles.Add(new Role
                {
                    Name = name,
                    Description = description
                });
            }
            else
            {
                role.Description = description;
            }
        }

        await context.SaveChangesAsync();

        var permissionIds = await context.Permissions
            .ToDictionaryAsync(permission => permission.Name, permission => permission.Id);
        var roleIds = await context.Roles
            .ToDictionaryAsync(role => role.Name, role => role.Id);

        var rolePermissions = new (string Role, string Permission)[]
        {
            ("User", "orders.view_own"),
            ("Seller", "orders.view_own"),
            ("Seller", "products.create"),
            ("Seller", "products.edit_own"),
            ("Packer", "orders.view_all"),
            ("Packer", "orders.update_status"),
            ("Support", "orders.view_all"),
            ("Support", "orders.update_status"),
            ("Support", "products.edit_any"),
            ("Support", "orders.force_status_change"),
            ("Moderator", "orders.view_own"),
            ("Moderator", "orders.view_all"),
            ("Moderator", "orders.update_status"),
            ("Moderator", "products.create"),
            ("Moderator", "products.edit_own"),
            ("Moderator", "products.edit_any"),
            ("Moderator", "products.delete"),
            ("Moderator", "orders.force_status_change"),
            ("Admin", "orders.view_own"),
            ("Admin", "orders.view_all"),
            ("Admin", "orders.update_status"),
            ("Admin", "products.create"),
            ("Admin", "products.edit_own"),
            ("Admin", "products.edit_any"),
            ("Admin", "products.delete"),
            ("Admin", "users.manage_roles"),
            ("Admin", "orders.force_status_change")
        };

        foreach (var (roleName, permissionName) in rolePermissions)
        {
            var roleId = roleIds[roleName];
            var permissionId = permissionIds[permissionName];
            if (!await context.RolePermissions.AnyAsync(item =>
                    item.RoleId == roleId && item.PermissionId == permissionId))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var categories = new (int Id, string Name, string Slug, string? ParentSlug)[]
        {
            (1, "Одежда", "clothing", null),
            (2, "Футболки", "tshirts", "clothing"),
            (3, "Худи", "hoodies", "clothing"),
            (4, "Брюки", "pants", "clothing"),
            (5, "Куртки", "jackets", "clothing"),
            (1001, "Обувь", "shoes", null),
            (1002, "Кроссовки", "sneakers", "shoes"),
            (1003, "Ботинки", "boots", "shoes"),
            (2001, "Аксессуары", "accessories", null),
            (2002, "Рюкзаки", "backpacks", "accessories"),
            (2003, "Шапки", "hats", "accessories")
        };

        var categoryIds = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (id, name, slug, parentSlug) in categories)
        {
            int? parentId = null;
            if (parentSlug is not null)
            {
                if (!categoryIds.TryGetValue(parentSlug, out var foundParentId))
                {
                    throw new InvalidOperationException(
                        $"Cannot seed category '{slug}': parent category '{parentSlug}' was not seeded first.");
                }

                parentId = foundParentId;
            }

            var category = await context.Categories
                .SingleOrDefaultAsync(item => item.Slug == slug);
            if (category is not null)
            {
                category.Name = name;
                category.ParentId = parentId;
                categoryIds[slug] = category.Id;
                await context.SaveChangesAsync();
                continue;
            }

            if (await context.Categories.AnyAsync(item => item.Id == id))
            {
                throw new InvalidOperationException(
                    $"Cannot seed category '{slug}': category ID {id} is already assigned to another category.");
            }

            category = new Category
            {
                Id = id,
                Name = name,
                Slug = slug,
                ParentId = parentId
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            categoryIds[slug] = category.Id;
        }

        await SynchronizeIdentitySequenceAsync(context, "Categories");
    }

    private static bool IsBcryptHash(string value)
    {
        return value.Length == 60
            && (value.StartsWith("$2a$", StringComparison.Ordinal)
                || value.StartsWith("$2b$", StringComparison.Ordinal)
                || value.StartsWith("$2y$", StringComparison.Ordinal));
    }

    private static async Task SynchronizeIdentitySequenceAsync(AppDbContext context, string table)
    {
        if (!context.Database.IsNpgsql())
        {
            return;
        }

        if (table is not ("Users" or "Categories" or "Products"))
        {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        var sql = table switch
        {
            "Users" =>
                """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Users" """,
            "Categories" =>
                """SELECT setval(pg_get_serial_sequence('"Categories"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Categories" """,
            "Products" =>
                """SELECT setval(pg_get_serial_sequence('"Products"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Products" """,
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };

        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
