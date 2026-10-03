// DTO для ответа с информацией о корзине

namespace Mish.Api.DTOs.Cart;

/// <summary>
/// DTO для ответа с информацией о корзине. [Response]
/// </summary>
public class CartResponse
{
    public List<CartItemResponse> Items { get; init; } = new();
    public int TotalItemsCount { get; init; }
    public decimal TotalPrice { get; init; }
}