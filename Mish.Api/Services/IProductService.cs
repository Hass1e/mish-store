namespace Mish.Api.Services;
using Mish.Api.Dtos;
using Mish.Api.Models;
using Microsoft.AspNetCore.Mvc;



public interface IProductService
{
    // Получение списка с пагинацией
    Task<object> GetAllAsync(int page, int size); 
    
    // Получение одного товара
    Task<ProductDetailDto?> GetByIdAsync(int id);
    
    // Создание товара (самое сложное)
    Task<ProductDetailDto> CreateAsync(CreateProductRequest request, IFormFile[]? images);
    
    // Обновление
    Task<bool> UpdateAsync(int id, Product product);
    
    // Удаление
    Task<bool> DeleteAsync(int id);
}