// Dtos/Categories/CategoryTreeDto.cs
using System.ComponentModel.DataAnnotations;

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

// Базовый класс, чтобы не дублировать валидацию
public abstract class CategoryCommandBase
{
    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug обязателен")]
    [MaxLength(50)]
    [RegularExpression(
        @"^[\p{L}\p{Nd}]+(?:-[\p{L}\p{Nd}]+)*$",
        ErrorMessage = "Slug может содержать только буквы, цифры и одиночные дефисы между ними.")]
    public string Slug { get; set; } = string.Empty;

    // NULL creates a root category; an ID creates a subcategory.
    public int? ParentId { get; set; }
}

public class CreateCategoryRequest : CategoryCommandBase
{
}

public class UpdateCategoryRequest : CategoryCommandBase
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}