using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Mish.Api.Models;

public class Category
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty; // Для красивых URL: "tshirts", "hoodies"
    
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    
    public ICollection<Category> Children { get; set; } = new List<Category>();
}