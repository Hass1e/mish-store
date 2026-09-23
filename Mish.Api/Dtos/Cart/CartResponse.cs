namespace Mish.Api.DTOs.Cart;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();
    public int TotalItemsCount { get; set; }
    public decimal TotalPrice { get; set; }
}