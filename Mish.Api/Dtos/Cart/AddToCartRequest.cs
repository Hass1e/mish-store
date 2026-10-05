// DTO для добавления товара в корзину

using System.ComponentModel.DataAnnotations;

namespace Mish.Api.DTOs.Cart;

public class AddToCartRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Укажите корректный товар")]
    public int ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
    public int Quantity { get; set; } = 1;
}