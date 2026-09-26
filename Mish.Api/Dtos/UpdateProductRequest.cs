// Dtos/Products/UpdateProductRequest.cs
namespace Mish.Api.Dtos;


public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Composition { get; set; } = string.Empty;
    public bool IsNew { get; set; }
    public bool IsHit { get; set; }
    public int Stock { get; set; }

    public List<string>? ImageUrls { get; set; } 
    public List<string>? Tags { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    // ❌ Никаких Id, CreatedAt, ImageUrls, Tags! 
    // Картинки и теги обновляются через отдельные эндпоинты или сервисы
    public List<string>? ExistingImages { get; set; }
}