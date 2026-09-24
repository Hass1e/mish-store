using Microsoft.EntityFrameworkCore;
using Mish.Api.Models;

namespace Mish.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Права (Permissions)
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                new() { Id = 1, Name = "orders.view_own", Description = "Просмотр своих заказов" },
                new() { Id = 2, Name = "orders.view_all", Description = "Просмотр всех заказов" },
                new() { Id = 3, Name = "orders.update_status", Description = "Изменение статуса заказа" },
                new() { Id = 4, Name = "products.create", Description = "Создание товаров" },
                new() { Id = 5, Name = "products.edit_own", Description = "Редактирование своих товаров" },
                new() { Id = 6, Name = "products.edit_any", Description = "Редактирование любых товаров" },
                new() { Id = 7, Name = "products.delete", Description = "Удаление товаров" },
                new() { Id = 8, Name = "users.manage_roles", Description = "Управление ролями пользователей" }
            };
            await context.Permissions.AddRangeAsync(permissions);
        }

        // 2. Роли (Roles)
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() { Id = 1, Name = "User", Description = "Покупатель" },
                new() { Id = 2, Name = "Seller", Description = "Продавец" },
                new() { Id = 3, Name = "Packer", Description = "Упаковщик/Транспортник" },
                new() { Id = 4, Name = "Support", Description = "Техподдержка" },
                new() { Id = 5, Name = "Moderator", Description = "Модератор" },
                new() { Id = 6, Name = "Admin", Description = "Администратор" }
            };
            await context.Roles.AddRangeAsync(roles);
        }

        // 3. Связи (RolePermissions)
        if (!await context.RolePermissions.AnyAsync())
        {
            var rolePermissions = new List<RolePermission>
            {
                // User: только свои заказы
                new() { RoleId = 1, PermissionId = 1 },
                
                // Seller: создание + редактирование СВОИХ товаров, просмотр заказов
                new() { RoleId = 2, PermissionId = 1 },
                new() { RoleId = 2, PermissionId = 4 },
                new() { RoleId = 2, PermissionId = 5 },
                
                // Packer: просмотр всех заказов + изменение статусов
                new() { RoleId = 3, PermissionId = 2 },
                new() { RoleId = 3, PermissionId = 3 },
                
                // Support: всё как у Packer + редактирование ЛЮБЫХ товаров
                new() { RoleId = 4, PermissionId = 2 },
                new() { RoleId = 4, PermissionId = 3 },
                new() { RoleId = 4, PermissionId = 6 },
                
                // Moderator: всё кроме управления ролями (8)
                new() { RoleId = 5, PermissionId = 1 },
                new() { RoleId = 5, PermissionId = 2 },
                new() { RoleId = 5, PermissionId = 3 },
                new() { RoleId = 5, PermissionId = 4 },
                new() { RoleId = 5, PermissionId = 5 },
                new() { RoleId = 5, PermissionId = 6 },
                new() { RoleId = 5, PermissionId = 7 },
                
                // Admin: ВСЁ
                new() { RoleId = 6, PermissionId = 1 },
                new() { RoleId = 6, PermissionId = 2 },
                new() { RoleId = 6, PermissionId = 3 },
                new() { RoleId = 6, PermissionId = 4 },
                new() { RoleId = 6, PermissionId = 5 },
                new() { RoleId = 6, PermissionId = 6 },
                new() { RoleId = 6, PermissionId = 7 },
                new() { RoleId = 6, PermissionId = 8 }
            };
            await context.RolePermissions.AddRangeAsync(rolePermissions);
        }

        await context.SaveChangesAsync();
    }
}