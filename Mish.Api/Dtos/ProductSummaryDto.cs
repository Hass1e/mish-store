namespace Mish.Api.Dtos;
public class ProductSummaryDto 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MainImageUrl { get; set; } = string.Empty; // Только ГЛАВНОЕ фото!
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public bool IsNew { get; set; }
    public bool IsHit { get; set; }
    public double Rating { get; set; } // Округленный рейтинг
}