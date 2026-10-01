using Mish.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mish.Api.Data;
using Mish.Api.Services;
using Moq;
using Microsoft.AspNetCore.Http;
using Mish.Api.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mish.Api.Tests.Services;

public class ProductServiceGetAllTests
{
    [Fact]
    public async Task CreateAsync_PersistsCategoryAndInactiveStatus()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Categories.Add(new Category
        {
            Id = 2003,
            Name = "Шапки",
            Slug = "hats"
        });
        await fixture.Context.SaveChangesAsync();

        await using var imageStream = new MemoryStream();
        using (var image = new Image<Rgba32>(1, 1))
        {
            await image.SaveAsPngAsync(imageStream);
        }
        imageStream.Position = 0;

        var imageFile = new FormFile(imageStream, 0, imageStream.Length, "images", "hat.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        var request = new CreateProductRequest
        {
            Name = "Тестовая шапка",
            Price = 100,
            CategoryId = 2003,
            IsActive = false
        };

        var created = await fixture.Service.CreateAsync(request, new[] { imageFile }, userId: 1);
        var saved = await fixture.Context.Products.FindAsync(created.Id);

        Assert.NotNull(saved);
        Assert.Equal(2003, saved.CategoryId);
        Assert.Equal("Шапки", saved.CategoryName);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public void ProductService_Constructor_CreatesUploadDirectory()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "Mish.Api.UnitTests", Guid.NewGuid().ToString("N"));
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AppDbContext(options);
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(contentRoot);
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSettings:BaseUrl"] = "http://localhost"
            })
            .Build();

        try
        {
            _ = new ProductService(context, environment.Object, configuration);
            Assert.True(Directory.Exists(Path.Combine(contentRoot, "wwwroot", "uploads")));
        }
        finally
        {
            if (Directory.Exists(contentRoot))
            {
                Directory.Delete(contentRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_CategoryIdZero_ReturnsOnlyProductsInThatCategory()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            new Product { Id = 1, Name = "Uncategorized", CategoryId = 0, IsActive = true },
            new Product { Id = 2, Name = "Hat", CategoryId = 5, IsActive = true },
            new Product { Id = 3, Name = "Inactive", CategoryId = 0, IsActive = false });
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.GetAllAsync(1, 20, null, categoryId: 0);
        var (items, total, hasMore) = ReadPage(result);

        var product = Assert.Single(items);
        Assert.Equal(1, product.Id);
        Assert.Equal(1, total);
        Assert.False(hasMore);
    }

    [Fact]
    public async Task GetAllAsync_Search_FiltersProductNamesWithoutCaseSensitivity()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            new Product { Id = 1, Name = "Blue Hat", IsActive = true },
            new Product { Id = 2, Name = "Red Scarf", IsActive = true });
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.GetAllAsync(1, 20, null, search: "hAt");
        var (items, total, _) = ReadPage(result);

        var product = Assert.Single(items);
        Assert.Equal(1, product.Id);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetAllAsync_SortPriceAscending_ReturnsProductsByPrice()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            new Product { Id = 1, Name = "Expensive", Price = 30, IsActive = true },
            new Product { Id = 2, Name = "Affordable", Price = 10, IsActive = true },
            new Product { Id = 3, Name = "Mid-range", Price = 20, IsActive = true });
        await fixture.Context.SaveChangesAsync();

        var result = await fixture.Service.GetAllAsync(1, 20, null, sort: "price_asc");
        var (items, _, _) = ReadPage(result);

        Assert.Equal(new[] { 2, 3, 1 }, items.Select(product => product.Id));
    }

    [Fact]
    public async Task GetAllAsync_Sqlite_SortPriceAscending_ReturnsProductsByPrice()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var root = Path.Combine(Path.GetTempPath(), "Mish.Api.UnitTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "wwwroot", "uploads"));
        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(value => value.ContentRootPath).Returns(root);
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiSettings:BaseUrl"] = "http://localhost"
                })
                .Build();
            var service = new ProductService(context, environment.Object, configuration);

            context.Products.AddRange(
                new Product { Id = 1, Name = "Expensive", Price = 30, IsActive = true },
                new Product { Id = 2, Name = "Affordable", Price = 10, IsActive = true },
                new Product { Id = 3, Name = "Mid-range", Price = 20, IsActive = true });
            await context.SaveChangesAsync();

            var result = await service.GetAllAsync(1, 20, null, sort: "price_asc");
            var (items, _, _) = ReadPage(result);

            Assert.Equal(new[] { 2, 3, 1 }, items.Select(product => product.Id));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllAsync_Sqlite_SearchMatchesCyrillicWithoutCaseSensitivity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var root = Path.Combine(Path.GetTempPath(), "Mish.Api.UnitTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "wwwroot", "uploads"));
        try
        {
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(value => value.ContentRootPath).Returns(root);
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiSettings:BaseUrl"] = "http://localhost"
                })
                .Build();
            var service = new ProductService(context, environment.Object, configuration);

            context.Products.AddRange(
                new Product { Id = 1, Name = "Шапка зимняя", IsActive = true },
                new Product { Id = 2, Name = "Кепка летняя", IsActive = true });
            await context.SaveChangesAsync();

            var result = await service.GetAllAsync(1, 20, null, search: "ШАПКА");
            var (items, total, _) = ReadPage(result);

            var product = Assert.Single(items);
            Assert.Equal(1, product.Id);
            Assert.Equal(1, total);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static (List<Mish.Api.Dtos.ProductSummaryDto> Items, int Total, bool HasMore)
        ReadPage(object result)
    {
        var type = result.GetType();
        return (
            (List<Mish.Api.Dtos.ProductSummaryDto>)type.GetProperty("items")!.GetValue(result)!,
            (int)type.GetProperty("total")!.GetValue(result)!,
            (bool)type.GetProperty("hasMore")!.GetValue(result)!
        );
    }
}
