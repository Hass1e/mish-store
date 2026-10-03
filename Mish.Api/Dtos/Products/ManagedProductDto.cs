namespace Mish.Api.Dtos;

/// <summary>
/// DTO для представления управляемого продукта.
/// </summary>
public sealed record ManagedProductDto(
    int Id,
    string Name,
    string? Sku,
    decimal Price,
    bool IsActive,
    DateTime UpdatedAt);
