using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Mish.Api.Extensions; // Твой неймспейс с хендлером
using System.Security.Claims;
using Xunit;

namespace Mish.Api.Tests;

public class PermissionTests
{
    [Theory]
    [InlineData("products.create")]
    [InlineData("products.edit_any")]
    [InlineData("products.delete")]
    public async Task AddPermissionPolicies_RegistersProductPolicy(string permission)
    {
        var services = new ServiceCollection();
        services.AddPermissionPolicies();
        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(permission);

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, requirement =>
            requirement is PermissionRequirement permissionRequirement &&
            permissionRequirement.Permission == permission);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithCommaSeparatedPermissions_ShouldSucceed()
    {
        // Arrange
        var handler = new PermissionRequirementHandler();
        var requirement = new PermissionRequirement("orders.view_all");
        
        // Имитируем токен, где права идут одной строкой через запятую
        var claims = new List<Claim> 
        { 
            new Claim("permissions", "products.create,orders.view_all,users.manage_roles") 
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded, "Политика должна быть успешно выполнена, так как право есть в строке");
    }

    [Fact]
    public async Task HandleRequirementAsync_MissingPermission_ShouldFail()
    {
        // Arrange
        var handler = new PermissionRequirementHandler();
        var requirement = new PermissionRequirement("orders.force_status_change");
        
        var claims = new List<Claim> 
        { 
            new Claim("permissions", "products.create,orders.view_all") // Нет force_status_change
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded, "Политика должна провалиться, так как права нет в строке");
    }
}