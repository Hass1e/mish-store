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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка связи: один пользователь → много товаров в корзине
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany() // Или .WithMany(u => u.CartItems), если добавил навигацию в User
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // При удалении юзера корзина тоже удаляется

        // Настройка связи: один товар → может быть во многих корзинах
        modelBuilder.Entity<CartItem>()
            .HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Не удаляем товар, если он в чьей-то корзине
    }




}
