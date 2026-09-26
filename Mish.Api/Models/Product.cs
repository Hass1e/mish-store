namespace Mish.Api.Models;


public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;       // Артикул товара
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }  = null;      // Скидка определяется автоматически
    public string Description { get; set; } = string.Empty; // Подробное описание товара
    public List<string> ImageUrls { get; set; } = new(); // Список URL изображений товара
    
    // Маркетинговые флаги
    public bool IsActive { get; set; } = false; // Товар доступен для продажи
    public bool IsNew { get; set; } = false; // Товар новый, можно использовать для баннеров "New"
    public bool IsHit { get; set; } = false; // Товар хит продаж, можно использовать для баннеров "Hit"
    
    // Характеристики
    public decimal Weight { get; set; } = 0; // Вес товара в граммах
    public string Composition { get; set; } = string.Empty; // Состав товара
    public List<string> Tags { get; set; } = new(); // Теги для поиска и фильтрации
    
    // Классификация
    public int CategoryId { get; set; } = 0; // ID категории, к которой принадлежит товар
    public string CategoryName { get; set; } = string.Empty; // Название категории, для удобства отображения
    
    // Статистика
    public int Bought { get; set; } = 0; // Количество раз, когда товар был куплен
    public int Stock { get; set; } = 0; // Количество товара на складе
    public int Rating { get; set; } = 0; // Средний рейтинг товара (например, от 1 до 5)
    public int Reviews { get; set; } = 0; // Количество отзывов о товаре

    public ICollection<ProductSeller> ProductSellers { get; set; } = new List<ProductSeller>();

    
    // Аудит
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}



