using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.DTOs;
using Mish.Api.Services;
using Mish.Api.Extensions;
namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(id!);
    }

    /// <summary>
    /// Создать заказ из текущей корзины пользователя.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(GetCurrentUserId());
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получить заказ по ID.
    /// Обычный пользователь — только свой.
    /// Админ/менеджер (orders.view_all) — любой.
    /// </summary>
 [HttpGet("{id:int}")]
public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
{
    var userId = GetCurrentUserId();
    
    // ✅ Используем наш кастомный метод вместо стандартного HasClaim
    var canViewAll = User.HasPermission("orders.view_all");
    
    var order = await _orderService.GetOrderByIdAsync(id, canViewAll ? null : userId);
    if (order == null) return NotFound();

    return Ok(order);
}

    /// <summary>
    /// Список заказов текущего пользователя.
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<OrderResponseDto>>> GetMyOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(GetCurrentUserId());
        return Ok(orders);
    }

    /// <summary>
    /// Список всех заказов (только для пользователей с orders.view_all).
    /// </summary>
    [HttpGet("all")]
    [Authorize(Policy = "orders.view_all")]
    public async Task<ActionResult<List<OrderResponseDto>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }


[HttpGet("admin")]
[Authorize(Policy = "orders.view_all")]
public async Task<ActionResult<List<AdminOrderResponseDto>>> GetAdminOrders([FromQuery] AdminOrdersFilterDto filter)
{
    var orders = await _orderService.GetAdminOrdersAsync(filter);
    return Ok(orders);
}



[HttpPut("{id:int}/status")]
[Authorize(Policy = "orders.update_status")]
public async Task<ActionResult<OrderResponseDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
{
    var userId = GetCurrentUserId();
    
    // Проверяем текущий статус для валидации
    // (В реальном проекте лучше вынести проверку в сервис, но оставим как было)
    var currentOrder = await _orderService.GetOrderByIdAsync(id, null);
    if (currentOrder == null) return NotFound();

    bool isNormalTransition = _orderService.IsStatusTransitionAllowed(currentOrder.Status, dto.Status);
    bool hasForcePermission = User.HasPermission("orders.force_status_change");

    if (!isNormalTransition && !hasForcePermission)
    {
        return Forbid("Для этого перехода требуется право 'orders.force_status_change'");
    }

    try
    {
        var order = await _orderService.UpdateStatusAsync(
            id, dto.Status, userId, canForce: !isNormalTransition && hasForcePermission);
            
        return Ok(order);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}






}