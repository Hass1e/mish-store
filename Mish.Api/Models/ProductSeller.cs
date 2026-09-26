using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mish.Api.Models;

[Table("ProductSellers")]
public class ProductSeller
{
    [Key]
    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;

    [Key]
    public int SellerId { get; set; }

    [ForeignKey(nameof(SellerId))]
    public User Seller { get; set; } = null!;

    public string RoleInProduct { get; set; } = "Owner"; // Owner, Editor, Viewer
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}