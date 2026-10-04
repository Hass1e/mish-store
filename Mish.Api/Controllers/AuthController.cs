// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Dtos;
using Mish.Api.Services;
using Mish.Api.Configuration;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt; // Для JwtRegisteredClaimNames
using System.Security.Claims;           // Для ClaimTypes
using Mish.Api.Attributes;
using Microsoft.AspNetCore.RateLimiting;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly AuthCookieSettings _authCookieSettings;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IOptions<AuthCookieSettings> authCookieSettings)
    {
        _authService = authService;
        _logger = logger;
        _authCookieSettings = authCookieSettings.Value;
    }


[HttpPost("register")] 
[AllowAnonymous]
[EnableRateLimiting("auth")] // Применяем Rate Limiting к регистрации
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
[TypeFilter(typeof(BruteForceProtectionAttribute))]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    try
    {
        var response = await _authService.LoginAsync(request);

        // ✅ ВАЖНО: Всегда возвращаем 401 при ошибке входа
        // Это триггерит BruteForceProtectionAttribute для увеличения счетчика
        if (response == null)
        {
            return Unauthorized(new { message = "Неверный логин или пароль" });
        }

        Response.Cookies.Append("accessToken", response.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _authCookieSettings.Secure, // Используем настройки из конфигурации appsettings.json
            SameSite = _authCookieSettings.SameSite,  
            Expires = DateTimeOffset.UtcNow.AddMinutes(_authCookieSettings.ExpirationMinutes),
            Path = "/",
        });

        return Ok(new 
        { 
            userId = response.UserId,
            username = response.Username,
            role = response.Role,
            accessToken = response.AccessToken
        });
    }
    catch (UnauthorizedAccessException ex)
    {
        // ⚠️ Логируем реальную причину на сервере
        _logger.LogWarning(ex, "Попытка входа с неверными учетными данными");
        
        // ✅ Клиенту отдаем универсальное сообщение (не раскрываем детали)
        return Unauthorized(new { message = "Неверный логин или пароль" });
    }
    catch (Exception ex)
    {
        // Ловим любые другие непредвиденные ошибки
        _logger.LogError(ex, "Критическая ошибка при попытке входа");
        return StatusCode(500, new { message = "Произошла внутренняя ошибка сервера" });
    }
}

    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = _authCookieSettings.Secure,
            SameSite = _authCookieSettings.SameSite,
            Path = "/"
        });

        return NoContent();
    }

// AuthController.cs
[HttpGet("me")]
[Authorize]
[EnableRateLimiting("fixed")]
public IActionResult GetCurrentUser()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var username = User.FindFirstValue(ClaimTypes.Name);
    var role = User.FindFirst(ClaimTypes.Role)?.Value;

    return Ok(new { userId, username, role });
}


[HttpGet("debug-cookies")]
[AllowAnonymous]
[EnableRateLimiting("fixed")]
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
