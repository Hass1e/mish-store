using Mish.Api.DTOs;

namespace Mish.Api.Services;

public interface IOrderService
{
    Task<OrderResponseDto> CreateOrderAsync(int userId);
    Task<OrderResponseDto?> GetOrderByIdAsync(int orderId, int? requestingUserId = null);
    Task<List<OrderResponseDto>> GetUserOrdersAsync(int userId);
    Task<List<OrderResponseDto>> GetAllOrdersAsync();
    Task<OrderResponseDto?> UpdateStatusAsync(int orderId, string newStatus, int changedByUserId, bool canForce = false);
    
    Task<List<AdminOrderResponseDto>> GetAdminOrdersAsync(AdminOrdersFilterDto filter);
    /// <summary>
    /// Проверяет, разрешен ли переход между статусами согласно бизнес-правилам.
    /// </summary>
     bool IsStatusTransitionAllowed(string currentStatus, string newStatus);
}