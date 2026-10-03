using System.ComponentModel.DataAnnotations;

namespace Mish.Api.Dtos;

/// <summary>
/// DTO для создания нового продукта.
/// </summary>
public class CreateProductRequest
{
    [Required(ErrorMessage = "Название товара обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    
    public List<string>? ImageUrls { get; set; } 

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0.01, 100000000, ErrorMessage = "Цена должна быть положительной и не превышать 100 млн")]
    public decimal Price { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Старая цена должна быть положительной")]
    public decimal? OldPrice { get; set; }

    public bool IsNew { get; set; }
    public bool IsHit { get; set; }
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Категория обязательна")]
    [Range(1, int.MaxValue, ErrorMessage = "Некорректный ID категории")]
    public int CategoryId { get; set; }
    
    [StringLength(2000, ErrorMessage = "Описание слишком длинное (макс. 2000 символов)")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Информация о составе слишком длинная")]
    public string? Composition { get; set; }
    
    // Для тегов лучше использовать массив строк, но валидировать их количество можно в сервисе
    public List<string>? Tags { get; set; }
    
    [Range(0, 100000, ErrorMessage = "Вес должен быть в разумных пределах")]
    public decimal? Weight { get; set; } = 0;

    [Range(0, 1000000, ErrorMessage = "Количество на складе не может быть отрицательным")]
    public int? Stock { get; set; } = 0;
}