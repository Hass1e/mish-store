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
    private readonly IUserService _userService;

    public CartController(ICartService cartService, IUserService userService)
    {
        _cartService = cartService;
        _userService = userService;
    }

    // GET: api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        var cart = await _cartService.GetCartAsync(userId.Value);
        return Ok(cart);
    }

    // POST: api/cart/add
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        try
        {
            var cart = await _cartService.AddToCartAsync(userId.Value, request);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("decrease/{productId:int}")]
    public async Task<IActionResult> DecreaseQuantity(int productId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        var cart = await _cartService.DecreaseQuantityAsync(userId.Value, productId);
        return Ok(cart);
    }

    // DELETE: api/cart/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFromCart(int id)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        await _cartService.RemoveFromCartAsync(userId.Value, id);
        return NoContent();
    }

    // DELETE: api/cart/clear
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        await _cartService.ClearCartAsync(userId.Value);
        return NoContent();
    }

    // DELETE: api/cart/remove-by-product/{productId}
    [HttpDelete("remove-by-product/{productId:int}")]
    public async Task<IActionResult> RemoveByProductId(int productId)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Unauthorized();
        await _cartService.RemoveByProductIdAsync(userId.Value, productId);
        return NoContent();
    }

    // Вспомогательный метод для получения ID из токена
    private async Task<int?> GetCurrentUserIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdClaim, out var userId)
            || await _userService.GetByIdAsync(userId) is null)
        {
            Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
            return null;
        }

        return userId;
    }

}