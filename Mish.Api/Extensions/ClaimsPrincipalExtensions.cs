using System.Security.Claims;

namespace Mish.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        var permissionsClaim = user.FindFirst("permissions")?.Value;
        
        if (string.IsNullOrEmpty(permissionsClaim))
            return false;

        var permissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return permissions.Contains(permission);
    }

    // Удобный метод для проверки нескольких прав (нужны ВСЕ)
    public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.All(p => user.HasPermission(p));
    }

    // Удобный метод для проверки (нужно ХОТЯ БЫ ОДНО)
    public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
    {
        return permissions.Any(p => user.HasPermission(p));
    }
}