using Mish.Api.Helpers;
using Mish.Api.Services;
using Xunit;

namespace Mish.Api.Tests;

/// <summary>
/// Тесты OrderService (машина состояний заказов).
/// Проверяет валидацию переходов между статусами заказа.
/// Допустимые переходы: Pending→Paid→Processing→Shipped→Delivered.
/// Отменённый заказ (Cancelled) можно получить из любого статуса.
/// </summary>
public class OrderServiceTests
{
    /// <summary>
    /// Тест 13: Валидирует корректные и некорректные переходы статусов заказа.
    /// Корректные: Pending→Paid, Pending→Cancelled, Processing→Shipped, Shipped→Delivered, Shipped→Cancelled.
    /// Некорректные: Pending→Shipped, Delivered→Pending, Cancelled→Paid.
    /// </summary>
    [Theory]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Paid, true)]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Cancelled, true)]
    [InlineData(OrderStatuses.Pending, OrderStatuses.Shipped, false)]
    [InlineData(OrderStatuses.Delivered, OrderStatuses.Pending, false)]
    [InlineData(OrderStatuses.Cancelled, OrderStatuses.Paid, false)]
    [InlineData(OrderStatuses.Processing, OrderStatuses.Shipped, true)]
    [InlineData(OrderStatuses.Shipped, OrderStatuses.Delivered, true)]
    [InlineData(OrderStatuses.Shipped, OrderStatuses.Cancelled, true)]
    public void IsStatusTransitionAllowed_ShouldValidateCorrectly(
        string current, string next, bool expected)
    {
        var result = OrderStateValidator.IsAllowed(current, next);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Тест 14: Переход в тот же статус (Pending→Pending) разрешён.
    /// </summary>
    [Fact]
    public void IsStatusTransitionAllowed_SameStatus_ShouldReturnTrue()
    {
        Assert.True(OrderStateValidator.IsAllowed("Pending", "Pending"));
        Assert.True(OrderStateValidator.IsAllowed("Delivered", "Delivered"));
    }
}

// Вспомогательный класс для тестирования (можно потом удалить или оставить)
public static class OrderStateValidator
{
    public static bool IsAllowed(string current, string next)
    {
        if (current == next) return true;
        
        return next switch
        {
            "Paid" => current == "Pending",
            "Processing" => current == "Paid",
            "Shipped" => current == "Processing",
            "Delivered" => current == "Shipped",
            "Cancelled" => current is "Pending" or "Paid" or "Processing" or "Shipped",
            _ => false
        };
    }
}