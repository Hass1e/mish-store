using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Services;
using Mish.Api.Dtos;
using System.Security.Claims;
using Mish.Api.Extensions;
namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Неверный токен" });

        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

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
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _userService.DeleteUserAsync(userId);
        
        Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
        
        return Ok(new { message = "Аккаунт удален" });
    }


     

}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}