using System.ComponentModel.DataAnnotations;

namespace Mish.Api.DTOs.Cart;

public class AddToCartRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; } = 1;
}