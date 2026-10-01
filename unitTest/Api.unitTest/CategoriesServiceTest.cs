using Microsoft.EntityFrameworkCore;
using Mish.Api.Data;
using Mish.Api.Models; // Убедись, что неймспейс совпадает с твоей моделью Category
using Mish.Api.Dtos;
using Mish.Api.Services;
using Xunit;

namespace Mish.Api.Tests;

public sealed class CategoriesServiceTests
{
    // Хелпер для создания изолированной In-Memory базы данных для каждого теста
    private static AppDbContext CreateDbContext(List<Category> categories)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникальное имя предотвращает пересечение данных между тестами
            .Options;

        var context = new AppDbContext(options);
        context.Categories.AddRange(categories);
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetTreeAsync_WithStandardHierarchy_ShouldBuildCorrectTree()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Одежда", Slug = "clothing", ParentId = null },
            new Category { Id = 2, Name = "Футболки", Slug = "tshirts", ParentId = 1 },
            new Category { Id = 3, Name = "Худи", Slug = "hoodies", ParentId = 1 },
            new Category { Id = 1001, Name = "Обувь", Slug = "shoes", ParentId = null }
        };
        var context = CreateDbContext(categories);
        var service = new CategoriesService(context);

        // Act
        var result = await service.GetTreeAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Два корневых элемента: Одежда и Обувь

        var clothing = result.FirstOrDefault(c => c.Name == "Одежда");
        Assert.NotNull(clothing);
        Assert.Equal(2, clothing.Children.Count); // У одежды два ребенка
        
        var tshirts = clothing.Children.FirstOrDefault(c => c.Name == "Футболки");
        Assert.NotNull(tshirts);
        Assert.Empty(tshirts.Children); // У футболок нет детей
    }

    [Fact]
    public async Task GetTreeAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var context = CreateDbContext(new List<Category>());
        var service = new CategoriesService(context);

        // Act
        var result = await service.GetTreeAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTreeAsync_WithDeepNesting_ShouldBuildFullHierarchy()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Корень", Slug = "root", ParentId = null },
            new Category { Id = 2, Name = "Уровень 1", Slug = "l1", ParentId = 1 },
            new Category { Id = 3, Name = "Уровень 2", Slug = "l2", ParentId = 2 },
            new Category { Id = 4, Name = "Уровень 3", Slug = "l3", ParentId = 3 }
        };
        var context = CreateDbContext(categories);
        var service = new CategoriesService(context);

        // Act
        var result = await service.GetTreeAsync(CancellationToken.None);

        // Assert
        var root = result.First();
        Assert.Equal("Корень", root.Name);
        
        var level1 = root.Children.First();
        Assert.Equal("Уровень 1", level1.Name);
        
        var level2 = level1.Children.First();
        Assert.Equal("Уровень 2", level2.Name);
        
        var level3 = level2.Children.First();
        Assert.Equal("Уровень 3", level3.Name);
        Assert.Empty(level3.Children);
    }

    [Fact]
    public async Task GetTreeAsync_ShouldRespectNameOrdering()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Zebra", Slug = "zebra", ParentId = null },
            new Category { Id = 2, Name = "Apple", Slug = "apple", ParentId = null },
            new Category { Id = 3, Name = "Mango", Slug = "mango", ParentId = null }
        };
        var context = CreateDbContext(categories);
        var service = new CategoriesService(context);

        // Act
        var result = await service.GetTreeAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Apple", result[0].Name);  // ToLookup сохраняет порядок исходной упорядоченной коллекции
        Assert.Equal("Mango", result[1].Name);
        Assert.Equal("Zebra", result[2].Name);
    }

    [Fact]
    public async Task GetTreeAsync_WithOrphanedCategories_ShouldNotCrashAndIgnoreOrphans()
    {
        // Arrange: Категория с ParentId = 999, которого не существует в БД
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Корень", Slug = "root", ParentId = null },
            new Category { Id = 2, Name = "Сирота", Slug = "orphan", ParentId = 999 } 
        };
        var context = CreateDbContext(categories);
        var service = new CategoriesService(context);

        // Act & Assert: ToLookup безопасно вернет пустую коллекцию для несуществующего ключа 999
        var exception = await Record.ExceptionAsync(() => service.GetTreeAsync(CancellationToken.None));
        Assert.Null(exception); // Исключений быть не должно

        var result = await service.GetTreeAsync(CancellationToken.None);
        Assert.Single(result); // Только "Корень", "Сирота" игнорируется, так как у нее нет валидного родителя в дереве
    }
}