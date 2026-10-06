// DTO для ответа с информацией о товаре в корзине
namespace Mish.Api.DTOs.Cart;

/// <summary>
/// DTO для ответа с информацией о товаре в корзине. [Response]
/// </summary>
public class CartItemResponse
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; } // Актуальная цена из Products
    public string? ImageUrl { get; init; } // Первая картинка
    public int Quantity { get; init; }
}