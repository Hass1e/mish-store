namespace Mish.Api.Services;
using Mish.Api.Dtos;
using Mish.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;



public interface IProductService
{
    // 1. Список товаров: нужен User, чтобы фильтровать (Админ видит всё, Продавец - только своё)
    Task<object> GetAllAsync(
        int page,
        int size,
        ClaimsPrincipal? user,
        int? categoryId = null,
        string? search = null,
        string? sort = null);
    
    // 2. Детали товара: нужен User, чтобы проверить доступ к чужому товару
    Task<ProductDetailDto?> GetByIdAsync(int id, ClaimsPrincipal? user);
    
    // 3. Создание: нужен UserId, чтобы автоматически добавить создателя в ProductSellers
    Task<ProductDetailDto> CreateAsync(CreateProductRequest request, IFormFile[]? images, int userId);

    Task<ProductDetailDto?> GetForEditAsync(int id, ClaimsPrincipal user);

    Task<object> GetManagedProductsAsync(int page, int size, ClaimsPrincipal user);
    
    // 4. Обновление: нужен User, чтобы проверить права (Owner или Admin?)
    Task<bool> UpdateAsync(int id, UpdateProductRequest request, IFormFile[]? images, ClaimsPrincipal user);
    
    // 5. Удаление: нужен User, чтобы запретить удалять чужие товары
    Task<bool> DeleteAsync(int id, ClaimsPrincipal user);
}