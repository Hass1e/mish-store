namespace Mish.Api.Dtos;

public class ManagedProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}