using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mish.Api.Dtos;
using Mish.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Mish.Api.Tests.Services;

public class ProductServiceUpdateTests
{
    [Fact]
    public async Task UpdateAsync_OwnerUpdatesText_KeepsExistingImages()
    {
        using var fixture = new ProductServiceTestFixture();
        var product = new Product
        {
            Id = 1,
            Name = "Old Name",
            Price = 100,
            ImageUrls = new List<string> { "/uploads/old.jpg" },
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = 5, RoleInProduct = "Owner" }
            }
        };
        fixture.Context.Products.Add(product);
        await fixture.Context.SaveChangesAsync();

        var owner = CreateUser(5, "products.edit_own");
        var request = new UpdateProductRequest
        {
            Name = "New Name",
            Price = 150
        };

        var result = await fixture.Service.UpdateAsync(1, request, null, owner);

        Assert.True(result);
        Assert.Equal("New Name", product.Name);
        Assert.Equal(150, product.Price);
        Assert.Equal(new[] { "/uploads/old.jpg" }, product.ImageUrls);
    }

    [Fact]
    public async Task UpdateAsync_OwnerAddsNewImage_AppendsItToExistingImages()
    {
        using var fixture = new ProductServiceTestFixture();
        var product = new Product
        {
            Id = 1,
            Name = "Product",
            Price = 100,
            ImageUrls = new List<string> { "/uploads/old.jpg" },
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = 5, RoleInProduct = "Owner" }
            }
        };
        fixture.Context.Products.Add(product);
        await fixture.Context.SaveChangesAsync();

        using var image = new Image<Rgba32>(1, 1);
        await using var content = new MemoryStream();
        await image.SaveAsPngAsync(content);
        content.Position = 0;
        var file = new FormFile(content, 0, content.Length, "images", "new.png");
        var request = new UpdateProductRequest
        {
            Name = "Product",
            Price = 100,
            ExistingImages = new List<string> { "/uploads/old.jpg" }
        };

        var result = await fixture.Service.UpdateAsync(
            1,
            request,
            new[] { file },
            CreateUser(5, "products.edit_own"));

        Assert.True(result);
        Assert.Equal(2, product.ImageUrls.Count);
        Assert.Contains("/uploads/old.jpg", product.ImageUrls);
        Assert.StartsWith("/uploads/", product.ImageUrls[1]);
    }

    [Fact]
    public async Task UpdateAsync_NonOwnerAttempt_ReturnsFalse()
    {
        using var fixture = new ProductServiceTestFixture();
        var product = new Product
        {
            Id = 1,
            Name = "Original",
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = 5, RoleInProduct = "Owner" }
            }
        };
        fixture.Context.Products.Add(product);
        await fixture.Context.SaveChangesAsync();

        var attacker = CreateUser(99, "products.edit_own");
        var request = new UpdateProductRequest { Name = "Hacked", Price = 1 };

        var result = await fixture.Service.UpdateAsync(1, request, null, attacker);

        Assert.False(result);
        Assert.Equal("Original", product.Name);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ReturnsFalse()
    {
        using var fixture = new ProductServiceTestFixture();
        var user = CreateUser(1, "products.edit_any");
        var request = new UpdateProductRequest { Name = "Test", Price = 10 };

        var result = await fixture.Service.UpdateAsync(999, request, null, user);

        Assert.False(result);
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
