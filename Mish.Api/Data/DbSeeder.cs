using Microsoft.EntityFrameworkCore;
using Mish.Api.Models;
using System.Security.Cryptography;

namespace Mish.Api.Data;

public static class DbSeeder
{
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

        if (table is not ("Users" or "Categories"))
        {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        var sql = table switch
        {
            "Users" =>
                """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Users" """,
            "Categories" =>
                """SELECT setval(pg_get_serial_sequence('"Categories"', 'Id'), COALESCE(MAX("Id"), 1), MAX("Id") IS NOT NULL) FROM "Categories" """,
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
