// CartController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Mish.Api.Services;
using Mish.Api.DTOs.Cart;
using Microsoft.AspNetCore.RateLimiting;

namespace Mish.Api.Controllers;



/// <summary>
/// Контроллер для управления корзиной пользователя. [Controller]
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Весь контроллер защищен!
[EnableRateLimiting("fixed")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // GET: api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    // POST: api/cart/add
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.AddToCartAsync(userId, request);
        return Ok(cart);
    }

    // DELETE: api/cart/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var userId = GetCurrentUserId();
        await _cartService.RemoveFromCartAsync(userId, id);
        return NoContent();
    }

    // DELETE: api/cart/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }

    // DELETE: api/cart/remove-by-product/{productId}
    [HttpDelete("remove-by-product/{productId:int}")]
    public async Task<IActionResult> RemoveByProductId(int productId)
    {
        var userId = GetCurrentUserId();
        await _cartService.RemoveByProductIdAsync(userId, productId);
        return NoContent();
    }

    // Вспомогательный метод для получения ID из токена
    private int GetCurrentUserId()
    {
        // ASP.NET обычно преобразует JWT claim "sub" в NameIdentifier.
        // Оставляем fallback для токенов, где преобразование отключено.
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Не удалось определить пользователя");
        }
        return userId;
    }

}