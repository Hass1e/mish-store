using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mish.Api.DTOs;
using Mish.Api.Helpers;
using Mish.Api.Models;
using Mish.Api.Data;

namespace Mish.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, ILogger<OrderService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(int userId)
    {
        // 1. Загружаем корзину с товарами
        var cartItems = await _db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
            throw new InvalidOperationException("Корзина пуста. Нельзя создать заказ.");

        // 2. Валидация наличия на складе
        foreach (var item in cartItems)
        {
            if (item.Product.Stock < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Товар \"{item.Product.Name}\" недоступен в количестве {item.Quantity}. " +
                    $"На складе: {item.Product.Stock} шт.");
            }
            if (!item.Product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Товар \"{item.Product.Name}\" недоступен для продажи.");
            }
        }

        // 3. Считаем сумму
        var totalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);

        // 4. Создаём заказ (в транзакции — всё или ничего)
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(); // получаем Order.Id

            // 5. Создаём позиции заказа (snapshot данных!)
            foreach (var cartItem in cartItems)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    Price = cartItem.Product.Price,
                    Quantity = cartItem.Quantity,
                     ImageUrl = cartItem.Product.ImageUrls.FirstOrDefault(),

                });

                // 6. Уменьшаем складской остаток
                cartItem.Product.Stock -= cartItem.Quantity;
                cartItem.Product.Bought += cartItem.Quantity;
            }

            // 7. Очищаем корзину пользователя
            _db.CartItems.RemoveRange(cartItems);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Заказ #{OrderId} создан пользователем {UserId} на сумму {Amount}",
                order.Id, userId, totalAmount);

            return await MapToResponseAsync(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId, int? requestingUserId = null)
    {
        var query = _db.Orders
            .Include(o => o.Items)
            .Where(o => o.Id == orderId);

        // Если передан userId — показываем только свои заказы
        if (requestingUserId.HasValue)
            query = query.Where(o => o.UserId == requestingUserId.Value);

        var order = await query.FirstOrDefaultAsync();
        if (order == null) return null;

        return await MapToResponseAsync(order);
    }

    public async Task<List<OrderResponseDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return (await Task.WhenAll(orders.Select(MapToResponseAsync))).ToList();
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return (await Task.WhenAll(orders.Select(MapToResponseAsync))).ToList();
    }

// Было: private static bool IsStatusTransitionAllowed(...)
// Стало:
public bool IsStatusTransitionAllowed(string currentStatus, string newStatus)
{
    if (currentStatus == newStatus) return true;
    var allowedTransitions = GetAllowedTransitions(currentStatus);
    return allowedTransitions.Contains(newStatus, StringComparer.OrdinalIgnoreCase);
}

// GetAllowedTransitions можно оставить приватным, он нужен только внутри сервиса
private static IReadOnlyList<string> GetAllowedTransitions(string currentStatus)
{
    return currentStatus switch
    {
        OrderStatuses.Pending    => new[] { OrderStatuses.Paid, OrderStatuses.Cancelled },
        OrderStatuses.Paid       => new[] { OrderStatuses.Processing, OrderStatuses.Cancelled },
        OrderStatuses.Processing => new[] { OrderStatuses.Shipped, OrderStatuses.Cancelled },
        OrderStatuses.Shipped    => new[] { OrderStatuses.Delivered, OrderStatuses.Cancelled },
        OrderStatuses.Delivered  => Array.Empty<string>(),
        OrderStatuses.Cancelled  => Array.Empty<string>(),
        _ => Array.Empty<string>()
    };
}

    public async Task<OrderResponseDto?> UpdateStatusAsync
        (
        int orderId, 
        string newStatus, 
        int changedByUserId, 
        bool canForce = false
        )
    {
        if (!OrderStatuses.IsValid(newStatus))
            throw new ArgumentException($"Недопустимый статус: {newStatus}");

        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return null;

        bool isAllowed = IsStatusTransitionAllowed(order.Status, newStatus);

        if (!isAllowed && !canForce)
        {
            throw new InvalidOperationException(
                $"Недопустимый переход: '{order.Status}' → '{newStatus}'.");
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Статус заказа #{OrderId} изменен: '{OldStatus}' → '{NewStatus}'. Пользователь: {UserId}",
            orderId, order.Status, newStatus, changedByUserId);

        return await MapToResponseAsync(order);
    }

    private async Task<OrderResponseDto> MapToResponseAsync(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity,
                Subtotal = i.Price * i.Quantity,
                ImageUrl = i.ImageUrl, 
            }).ToList()
        };
    }


    public async Task<List<AdminOrderResponseDto>> GetAdminOrdersAsync(AdminOrdersFilterDto filter)
    {
    // Ограничиваем Take, чтобы нельзя было вытащить всю БД одним запросом (макс 100)
    var take = Math.Min(Math.Max(filter.Take, 1), 100);

    var query = _db.Orders
        .Include(o => o.User) // Подгружаем юзера для Username
        .Include(o => o.Items)
        .AsQueryable();

    // Фильтр по статусу
    if (!string.IsNullOrWhiteSpace(filter.Status))
    {
        query = query.Where(o => o.Status == filter.Status);
    }

    // Поиск по ID заказа или Имени пользователя
    if (!string.IsNullOrWhiteSpace(filter.Search))
    {
        if (int.TryParse(filter.Search, out int searchId))
        {
            query = query.Where(o => o.Id == searchId || o.User.Username.Contains(filter.Search));
        }
        else
        {
            query = query.Where(o => o.User.Username.Contains(filter.Search));
        }
    }

    // Запрашиваем take + 1, чтобы понять, есть ли следующая страница
    var orders = await query
        .OrderByDescending(o => o.CreatedAt)
        .Skip(filter.Skip)
        .Take(take + 1)
        .ToListAsync();

    // Маппинг в DTO
    return orders.Select(o => new AdminOrderResponseDto
    {
        Id = o.Id,
        UserId = o.UserId,
        UserUsername = o.User?.Username ?? "Неизвестно",
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Items = o.Items.Select(i => new OrderItemResponseDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ImageUrl = i.ImageUrl,
            Price = i.Price,
            Quantity = i.Quantity,
            Subtotal = i.Price * i.Quantity
        }).ToList()
    }).ToList();

    }




}