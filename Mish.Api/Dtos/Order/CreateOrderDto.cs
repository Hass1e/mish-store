namespace Mish.Api.DTOs;

/// <summary>
/// Пустой DTO — заказ создаётся из корзины пользователя,
/// дополнительных полей от клиента не требуется.
/// Но оставляем класс для единообразия и будущего расширения
/// (адрес доставки, комментарий, промокод и т.д.).
/// </summary>
public class CreateOrderDto
{
    // Пока пусто. В будущем сюда можно добавить:
    // public string? DeliveryAddress { get; set; }
    // public string? Comment { get; set; }
    // public string? PromoCode { get; set; }
}

