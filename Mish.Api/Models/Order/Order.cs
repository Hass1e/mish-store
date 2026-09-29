using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace Mish.Api.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Общая сумма заказа. Рассчитывается на бэкенде при создании.
    /// </summary>
    [Precision(18,2)]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Статусы: Pending, Paid, Processing, Shipped, Delivered, Cancelled
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}