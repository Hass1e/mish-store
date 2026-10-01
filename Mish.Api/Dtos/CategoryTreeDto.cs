// Dtos/Categories/CategoryTreeDto.cs

namespace Mish.Api.Dtos;

public class CategoryTreeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<CategoryTreeDto> Children { get; set; } = new();
}

// Dtos/Products/ProductsFilterDto.cs
public class ProductsFilterDto
{
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; } // "price_asc", "price_desc", "name", "rating", "new", "hit"
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}