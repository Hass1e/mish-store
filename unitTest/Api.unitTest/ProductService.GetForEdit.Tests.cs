using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mish.Api.Models;
using Xunit;

namespace Mish.Api.Tests.Services;

/// <summary>
/// Тесты ProductService: метод GetForEditAsync.
/// Проверяет логику доступа к редактированию товаров.
/// Права: products.edit_any (админ) vs products.edit_own (владелец).
/// </summary>
public class ProductServiceGetForEditTests
{
    /// <summary>
    /// Тест 25: Админ (products.edit_any) может редактировать любой товар.
    /// </summary>
    [Fact]
    public async Task GetForEditAsync_Admin_CanEditAnyProductEvenWithoutOwner()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.Add(new Product
        {
            Id = 99,
            Name = "Legacy Product",
            ProductSellers = new List<ProductSeller>()
        });
        await fixture.Context.SaveChangesAsync();

        var admin = CreateUser(1, "products.edit_any");
        var result = await fixture.Service.GetForEditAsync(99, admin);

        Assert.NotNull(result);
        Assert.Equal("Legacy Product", result.Name);
    }

    /// <summary>
    /// Тест 26: Владелец товара может редактировать свой товар.
    /// </summary>
    [Fact]
    public async Task GetForEditAsync_Owner_CanEditOwnProduct()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.Add(new Product
        {
            Id = 10,
            Name = "My Product",
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = 5, RoleInProduct = "Owner" }
            }
        });
        await fixture.Context.SaveChangesAsync();

        var owner = CreateUser(5, "products.edit_own");
        var result = await fixture.Service.GetForEditAsync(10, owner);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
    }

    /// <summary>
    /// Тест 27: Не-владелец получает null при попытке редактирования чужого товара.
    /// </summary>
    [Fact]
    public async Task GetForEditAsync_NonOwnerSeller_CannotEdit_ReturnsNull()
    {
        using var fixture = new ProductServiceTestFixture();
        fixture.Context.Products.Add(new Product
        {
            Id = 10,
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = 5, RoleInProduct = "Owner" }
            }
        });
        await fixture.Context.SaveChangesAsync();

        var otherSeller = CreateUser(8, "products.edit_own");
        var result = await fixture.Service.GetForEditAsync(10, otherSeller);

        Assert.Null(result);
    }

    /// <summary>
    /// Тест 28: Запрос на несуществующий товар возвращает null.
    /// </summary>
    [Fact]
    public async Task GetForEditAsync_ProductNotFound_ReturnsNull()
    {
        using var fixture = new ProductServiceTestFixture();
        var admin = CreateUser(1, "products.edit_any");

        var result = await fixture.Service.GetForEditAsync(999, admin);

        Assert.Null(result);
    }

    private static ClaimsPrincipal CreateUser(int userId, string permission)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("permissions", permission)
        }, "Test"));
    }
}
