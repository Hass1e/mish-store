// ProductDetailDto.cs
namespace Mish.Api.Dtos;

/// <summary>
/// DTO для представления детальной информации о продукте. [Response]
/// </summary>
public class ProductDetailDto : ProductSummaryDto // Наследуем базовое
{
    public string Description { get; init; } = string.Empty;
    public List<string> ImageUrls { get; init; } = new(); // ВСЕ фото
    public string Composition { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = new();
    public decimal Weight { get; init; }
    public int Stock { get; init; }
    public string SKU { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int CategoryId { get; init; }
    // ... все остальные 30 полей
}