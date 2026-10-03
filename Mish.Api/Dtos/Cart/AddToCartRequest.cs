// DTO для добавления товара в корзину

using System.ComponentModel.DataAnnotations;

namespace Mish.Api.DTOs.Cart;

public class AddToCartRequest
{
    public int ProductId { get; set; }

    [Range(0, 100, ErrorMessage = "Количество должно быть от 0 до 100")]
    public int Quantity { get; set; } = 1;
}