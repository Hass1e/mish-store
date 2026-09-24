using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;             // ← И ЭТУ, если её нет
namespace Mish.Api.Extensions;

// Простой атрибут, который хранит название требуемого права
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission; // Используем стандартный механизм политик ASP.NET Core
    }
}

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}

// Обработчик, который проверяет наш кастомный claim "permissions"
public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissionsClaim = context.User.FindFirst("permissions")?.Value;
        
        if (string.IsNullOrEmpty(permissionsClaim))
            return Task.CompletedTask;

        var userPermissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        // Если право пользователя совпадает с требуемым в атрибуте — успех!
        if (userPermissions.Contains(requirement.Permission, StringComparer.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

// Метод расширения для регистрации в Program.cs
public static class PermissionPolicyRegistrar
{
    public static void AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("users.manage_roles", policy =>
                policy.Requirements.Add(new PermissionRequirement("users.manage_roles")));

            options.AddPolicy("products.edit_any", policy =>
                policy.Requirements.Add(new PermissionRequirement("products.edit_any")));
        });

        services.AddSingleton<IAuthorizationHandler, PermissionRequirementHandler>();
    }
}