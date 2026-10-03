using System.ComponentModel.DataAnnotations;

namespace Mish.Api.DTOs;

// DTO для ответа пользователю
public class OrderResponseDto
{
    public int Id { get; init; }
    public int UserId { get; init; } 
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<OrderItemResponseDto> Items { get; init; } = new();
}

// DTO для товара внутри заказа
public class OrderItemResponseDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public decimal Subtotal { get; init; }
    public string? ImageUrl { get; init; }
}

// DTO для админа (расширенный)
public class AdminOrderResponseDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string UserUsername { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<OrderItemResponseDto> Items { get; init; } = new();
}

// DTO для фильтров (ВХОДЯЩИЕ ДАННЫЕ - тут нужна защита!)
public class AdminOrdersFilterDto
{
    [Range(0, int.MaxValue)]
    public int Skip { get; set; } = 0;

    [Range(1, 100, ErrorMessage = "Нельзя запросить более 100 записей за раз")]
    public int Take { get; set; } = 20;

    public string? Status { get; set; }
    
    [StringLength(50, ErrorMessage = "Слишком длинный поисковый запрос")]
    public string? Search { get; set; }
}