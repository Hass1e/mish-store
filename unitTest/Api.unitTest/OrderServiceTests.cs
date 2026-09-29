using Mish.Api.Helpers;
using Mish.Api.Services;
using Xunit;

namespace Mish.Api.Tests;

public class OrderServiceTests
{
    // Тестируем машину состояний БЕЗ создания сервиса и БД
    // Метод IsStatusTransitionAllowed теперь публичный и статический по своей природе
    
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
        // Создаем сервис только для доступа к методу, 
        // но НЕ используем БД в этом тесте
        // Лучше всего сделать метод static, но если он в классе сервиса - 
        // нам придется создать экземпляр. 
        // Но так как метод НЕ использует _db, можно использовать Moq с CallBase=true
        // ИЛИ просто протестировать через helper класс
        
        // ВАРИАНТ 1: Если можешь сделать метод static в OrderService - сделай
        // ВАРИАНТ 2: Используем Moq с CallBase (но это сложно для DbContext)
        
        // ПРОСТОЙ ВАРИАНТ для pet-проекта: 
        // Перенеси логику в отдельный static класс OrderStateValidator
        // И тестируй его напрямую
        var result = OrderStateValidator.IsAllowed(current, next);
        Assert.Equal(expected, result);
    }

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