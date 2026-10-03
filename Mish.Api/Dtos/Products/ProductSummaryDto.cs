namespace Mish.Api.Dtos;

/// <summary>
/// DTO для представления краткой информации о продукте.[Response]
/// </summary>
public class ProductSummaryDto 
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string MainImageUrl { get; init; } = string.Empty; // Только ГЛАВНОЕ фото!
    public decimal Price { get; init; }
    public decimal? OldPrice { get; init; }
    public bool IsNew { get; init; }
    public bool IsHit { get; init; }
    public double Rating { get; init; } // Округленный рейтинг
}