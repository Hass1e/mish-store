using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Mish.Api.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Snapshot: название товара на момент покупки.
    /// Защищает историю заказа от изменений/удаления товара в каталоге.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot: цена за единицу на момент покупки.
    /// Не ссылается на Product.Price — тот может измениться.
    /// </summary>
    [Precision(18,2)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [MaxLength(200)]
    public string? ImageUrl { get; set; }
}