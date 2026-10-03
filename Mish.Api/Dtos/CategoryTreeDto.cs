// Dtos/Categories/CategoryTreeDto.cs

namespace Mish.Api.Dtos;

/// <summary>
/// DTO для представления дерева категорий.
/// </summary>
public class CategoryTreeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public List<CategoryTreeDto> Children { get; init; } = new();
}
// <summary>
// DTO для представления дерева категорий. [Request]
// </summary>
// Dtos/Products/ProductsFilterDto.cs
public class ProductsFilterDto
{
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; } // "price_asc", "price_desc", "name", "rating", "new", "hit"
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}