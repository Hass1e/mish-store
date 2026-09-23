
namespace Mish.Api.Dtos;

public class CreateProductRequest
{

    public string Name { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new(); 
    
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    
    public bool IsNew { get; set; }
    public bool IsHit { get; set; }
    
    // ✅ Rating вычисляется на основе отзывов, не передается при создании
    
    public string Description { get; set; } = string.Empty;
    public string? Composition { get; set; } = string.Empty;
    
    public List<string> Tags { get; set; } = new();
    
    public decimal? Weight { get; set; } = 0;
    public int? Stock { get; set; } = 0;
    
    // ✅ SKU можно генерировать на бэкенде (например: "MISH-{Guid}")
}