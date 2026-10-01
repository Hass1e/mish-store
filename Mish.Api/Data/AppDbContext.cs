using Microsoft.EntityFrameworkCore;
using Mish.Api.Models;
using System.Reflection;

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

    public DbSet<Category> Categories { get; set; }

public DbSet<Role> Roles => Set<Role>();
public DbSet<Permission> Permissions => Set<Permission>();
public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

public DbSet<Order> Orders { get; set; }
public DbSet<OrderItem> OrderItems { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

        modelBuilder.HasDbFunction(
                typeof(AppDbContext).GetMethod(nameof(UnicodeLower), BindingFlags.Public | BindingFlags.Static)!)
            .HasName("unicode_lower");

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

          modelBuilder.Entity<ProductSeller>()
        .HasKey(ps => new { ps.ProductId, ps.SellerId }); // Явный составной ключ

    modelBuilder.Entity<ProductSeller>()
        .HasOne(ps => ps.Product)
        .WithMany(p => p.ProductSellers)
        .HasForeignKey(ps => ps.ProductId)
        .OnDelete(DeleteBehavior.Cascade); // При удалении товара удаляются связи

    modelBuilder.Entity<ProductSeller>()
        .HasOne(ps => ps.Seller)
        .WithMany() // У User может не быть навигации ProductSellers, это нормально
        .HasForeignKey(ps => ps.SellerId)
        .OnDelete(DeleteBehavior.Restrict); // Не удаляем продавца, если у него есть товары



   

    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.Status);
        entity.Property(e => e.Status).HasMaxLength(20);

        entity.HasOne(e => e.User)
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict); // не удаляем заказы при удалении юзера
    });

    modelBuilder.Entity<OrderItem>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.OrderId);
        entity.HasIndex(e => e.ProductId);
        entity.Property(e => e.ProductName).HasMaxLength(200);

        entity.HasOne(e => e.Order)
              .WithMany(o => o.Items)
              .HasForeignKey(e => e.OrderId)
              .OnDelete(DeleteBehavior.Cascade); // при удалении заказа — удаляются позиции
    });

}

    public static string UnicodeLower(string value) => value.ToLowerInvariant();
}
