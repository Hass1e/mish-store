using Mish.Api.DTOs.Cart;

namespace Mish.Api.Services;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(int userId);
    Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartResponse> DecreaseQuantityAsync(int userId, int productId);
    Task RemoveFromCartAsync(int userId, int cartItemId);
    Task RemoveByProductIdAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}