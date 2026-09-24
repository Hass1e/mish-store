using Microsoft.EntityFrameworkCore;
using Mish.Api.Models;

namespace Mish.Api.Data;

// Этот класс связывает твои C# классы (модели) с таблицами в базе данных
public class AppDbContext : DbContext
{
    // Конструктор нужен, чтобы ASP.NET мог передать сюда настройки подключения
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Это "таблица" Products в базе данных. 
    // EF Core сам создаст её по структуре класса Product
    public DbSet<Product> Products { get; set; }

    // Data/AppDbContext.cs
    public DbSet<User> Users { get; set; }

    public DbSet<CartItem> CartItems => Set<CartItem>();

public DbSet<Role> Roles => Set<Role>();
public DbSet<Permission> Permissions => Set<Permission>();
public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

        // Настройка связи: один пользователь → много товаров в корзине
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Настройка связи: один товар → может быть во многих корзинах
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

    // Уникальный индекс: одна роль не может иметь дубликат права
    modelBuilder.Entity<RolePermission>()
        .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
        .IsUnique();

    // Каскадное удаление: при удалении роли удаляются её связи с правами
    modelBuilder.Entity<RolePermission>()
        .HasOne(rp => rp.Role)
        .WithMany(r => r.RolePermissions)
        .HasForeignKey(rp => rp.RoleId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<RolePermission>()
        .HasOne(rp => rp.Permission)
        .WithMany(p => p.RolePermissions)
        .HasForeignKey(rp => rp.PermissionId)
        .OnDelete(DeleteBehavior.Restrict); // Не удаляем право, если оно используется
}


}
