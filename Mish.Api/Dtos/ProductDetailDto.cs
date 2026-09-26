// ProductDetailDto.cs
namespace Mish.Api.Dtos;
public class ProductDetailDto : ProductSummaryDto // Наследуем базовое
{
    public string Description { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new(); // ВСЕ фото
    public string Composition { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public decimal Weight { get; set; }
    public int Stock { get; set; }
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    // ... все остальные 30 полей
}