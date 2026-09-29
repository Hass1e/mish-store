namespace Mish.Api.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public string? ImageUrl { get; set; }
}

// DTO для ответа админу (включает данные пользователя)
public class AdminOrderResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserUsername { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

// DTO для параметров запроса (фильтры и пагинация)
public class AdminOrdersFilterDto
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20; // По умолчанию 20
    public string? Status { get; set; }
    public string? Search { get; set; } // Поиск по ID заказа или Username
}