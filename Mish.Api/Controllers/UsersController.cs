using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Services;
using Mish.Api.Dtos;
using System.Security.Claims;
using Mish.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = GetCurrentUserIdClaim();

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Неверный токен" });

        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null)
        {
            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            return Unauthorized(new { message = "Сессия устарела. Войдите снова." });
        }

        return Ok(new 
        { 
            userId = user.Id,
            username = user.Username,
            role = user.Role,
            createdAt = user.CreatedAt.ToString("o")
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting("auth")] // Применяем более строгий Rate Limiting к изменению пароля
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = GetCurrentUserIdClaim();

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (await _userService.GetByIdAsync(userId) is null)
        {
            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            return Unauthorized(new { message = "Сессия устарела. Войдите снова." });
        }

        try
        {
            await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            return Ok(new { message = "Пароль успешно изменен" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdClaim = GetCurrentUserIdClaim();

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (await _userService.GetByIdAsync(userId) is null)
        {
            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            return Unauthorized(new { message = "Сессия устарела. Войдите снова." });
        }

        await _userService.DeleteUserAsync(userId);
        
        Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
        
        return Ok(new { message = "Аккаунт удален" });
    }

    private string? GetCurrentUserIdClaim() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}