using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Models;

public class CartItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    
    // Навигационное свойство для EF Core (не колонка в БД!)
    public User User { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }
    
    // Навигационное свойство для EF Core (не колонка в БД!)
    public Product Product { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}