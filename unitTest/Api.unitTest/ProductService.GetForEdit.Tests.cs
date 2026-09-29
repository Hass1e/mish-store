using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mish.Api.Models;
using Xunit;

namespace Mish.Api.Tests.Services;

public class ProductServiceGetForEditTests
{
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
