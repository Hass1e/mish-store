using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mish.Api.Models;
using Xunit;

namespace Mish.Api.Tests.Services;

/// <summary>
/// Тесты ProductService: метод GetManagedProductsAsync.
/// Проверяет фильтрацию товаров по правам доступа и пагинацию.
/// Админ видит все товары, продавец — только свои.
/// </summary>
public class ProductServiceGetManagedTests
{
    /// <summary>
    /// Тест 29: Админ видит все товары в системе.
    /// </summary>
    [Fact]
    public async Task GetManagedProductsAsync_AdminUser_ReturnsAllProducts()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Product 1",
                ProductSellers = new List<ProductSeller> { new() { SellerId = 2 } }
            },
            new Product
            {
                Id = 2,
                Name = "Product 2",
                ProductSellers = new List<ProductSeller> { new() { SellerId = 3 } }
            });
        await fixture.Context.SaveChangesAsync();

        var admin = CreateUser(1, "products.edit_any");
        var result = await fixture.Service.GetManagedProductsAsync(1, 10, admin);
        var (items, total, _, _, _) = ReadPage(result);

        Assert.Equal(2, items.Count);
        Assert.Equal(2, total);
    }

    /// <summary>
    /// Тест 30: Продавец видит только свои товары.
    /// </summary>
    [Fact]
    public async Task GetManagedProductsAsync_SellerUser_ReturnsOnlyOwnedProducts()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "My Product",
                ProductSellers = new List<ProductSeller>
                {
                    new() { SellerId = 2, RoleInProduct = "Owner" }
                }
            },
            new Product
            {
                Id = 2,
                Name = "Other Product",
                ProductSellers = new List<ProductSeller>
                {
                    new() { SellerId = 99, RoleInProduct = "Owner" }
                }
            });
        await fixture.Context.SaveChangesAsync();

        var seller = CreateUser(2, "products.edit_own");
        var result = await fixture.Service.GetManagedProductsAsync(1, 10, seller);
        var (items, total, _, _, _) = ReadPage(result);

        var product = Assert.Single(items);
        Assert.Equal("My Product", product.Name);
        Assert.Equal(1, total);
    }

    /// <summary>
    /// Тест 31: Пользователь без валидного userId в claims не видит товары.
    /// </summary>
    [Fact]
    public async Task GetManagedProductsAsync_InvalidUserClaim_ReturnsNoProducts()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.Add(new Product { Id = 1, Name = "Hidden from invalid identity" });
        await fixture.Context.SaveChangesAsync();

        var invalidUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permissions", "products.edit_own")
        }, "Test"));

        var result = await fixture.Service.GetManagedProductsAsync(1, 10, invalidUser);
        var (items, total, _, _, _) = ReadPage(result);

        Assert.Empty(items);
        Assert.Equal(0, total);
    }

    /// <summary>
    /// Тест 32: Пагинация рассчитывается корректно (page, size, totalPages).
    /// При 15 товарах и размере 10: 2 страницы, на второй — 5 товаров.
    /// </summary>
    [Fact]
    public async Task GetManagedProductsAsync_Pagination_CalculatesCorrectly()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.AddRange(
            Enumerable.Range(1, 15).Select(id => new Product { Id = id, Name = $"Product {id}" }));
        await fixture.Context.SaveChangesAsync();

        var admin = CreateUser(1, "products.edit_any");
        var result = await fixture.Service.GetManagedProductsAsync(2, 10, admin);
        var (items, total, page, size, totalPages) = ReadPage(result);

        Assert.Equal(5, items.Count);
        Assert.Equal(15, total);
        Assert.Equal(2, page);
        Assert.Equal(10, size);
        Assert.Equal(2, totalPages);
    }

    private static ClaimsPrincipal CreateUser(int userId, string permission)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("permissions", permission)
        }, "Test"));
    }

    private static (List<Mish.Api.Dtos.ManagedProductDto> Items, int Total, int Page, int Size, int TotalPages)
        ReadPage(object result)
    {
        var type = result.GetType();
        return (
            (List<Mish.Api.Dtos.ManagedProductDto>)type.GetProperty("items")!.GetValue(result)!,
            (int)type.GetProperty("total")!.GetValue(result)!,
            (int)type.GetProperty("page")!.GetValue(result)!,
            (int)type.GetProperty("size")!.GetValue(result)!,
            (int)type.GetProperty("totalPages")!.GetValue(result)!
        );
    }
}
