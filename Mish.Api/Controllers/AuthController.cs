// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Dtos;
using Mish.Api.Services;
using System.IdentityModel.Tokens.Jwt; // Для JwtRegisteredClaimNames
using System.Security.Claims;           // Для ClaimTypes

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


[HttpPost("register")] 
[AllowAnonymous]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    try
    {
        var user = await _authService.RegisterAsync(request);
        
        // ✅ Генерируем токен СРАЗУ после регистрации (используем приватный метод сервиса)
        // Для этого нужно сделать GenerateJwtToken публичным или создать отдельный метод
        // Самый чистый вариант: добавить метод RegisterWithTokenAsync в сервис
        
        return Ok(new { 
            message = "Пользователь успешно зарегистрирован, теперь войдите в систему", 
            username = user.Username,
            role = user.Role,
            createdAt = user.CreatedAt
        });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // ⚠️ Убедись, что метод LoginAsync реализован в AuthService!
            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            Response.Cookies.Append("accessToken", response.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,           // Обязательно false для http://localhost
                SameSite = SameSiteMode.Unspecified, // Lax — самый безопасный компромисс для localhost
                Expires = DateTime.UtcNow.AddMinutes(4320),
                Path = "/",
                
            });

        return Ok(new 
        { 
            userId = response.UserId,
            username = response.Username,
            role = response.Role,
            accessToken = response.AccessToken // Токен должен быть доступен фронтенду для fallback
            
        });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Unauthorized(new { message = ex.Message });
    }
}

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Unspecified,
            Path = "/"
        });

        return NoContent();
    }

// AuthController.cs
[HttpGet("me")]
[Authorize]
public IActionResult GetCurrentUser()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var username = User.FindFirstValue(ClaimTypes.Name);
    var role = User.FindFirst(ClaimTypes.Role)?.Value;

    return Ok(new { userId, username, role });
}


[HttpGet("debug-cookies")]
[AllowAnonymous]
public IActionResult DebugCookies()
{
    var cookies = Request.Cookies;
    return Ok(new { 
        count = cookies.Count,
        keys = cookies.Keys.ToList(),
        accessToken = cookies["accessToken"] 
    });
}




}
