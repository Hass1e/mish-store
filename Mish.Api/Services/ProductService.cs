namespace Mish.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // Нужно для IWebHostEnvironment
using Mish.Api.Data;
using Mish.Api.Dtos;
using Mish.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;



public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public ProductService(AppDbContext context, IWebHostEnvironment env, IConfiguration config)
    {
        _context = context;
        _uploadsPath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
         _baseUrl = config["ApiSettings:BaseUrl"]   ?? throw new InvalidOperationException("Ключ ApiSettings:BaseUrl отсутствует в конфигурации.");
    }

    public async Task<object> GetAllAsync(int page, int size)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 50;

        var total = await _context.Products.CountAsync();
        
        var products = await _context.Products
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                OldPrice = p.OldPrice,
                MainImageUrl = p.ImageUrls.FirstOrDefault() ?? $"{_baseUrl}/uploads/placeholder.jpg",
                IsNew = p.IsNew,
                IsHit = p.IsHit,
                Rating = (double)p.Rating
            })
            .ToListAsync();

        return new {
            items = products,
            total = total,
            page = page,
            size = size,
            totalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null;

        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Description = product.Description,
            ImageUrls = product.ImageUrls ?? new List<string>(),
            MainImageUrl = product.ImageUrls?.FirstOrDefault() ?? $"{_baseUrl}/uploads/placeholder.jpg",
            Rating = product.Rating,
            Stock = product.Stock,
            SKU = product.SKU,
            Weight = product.Weight,
            Composition = product.Composition
        };
    }

    public async Task<ProductDetailDto> CreateAsync(CreateProductRequest request, IFormFile[]? images)
    {
        // Валидация текста (можно вынести в отдельный метод, но пока оставим тут для простоты)
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 6 || request.Name.Length > 32)
            throw new ArgumentException("Название должно быть от 6 до 32 символов.");
            
        if (request.Price <= 0)
            throw new ArgumentException("Цена должна быть больше нуля.");

        List<string> savedUrls = new();
        if (images != null && images.Length > 0)
        {
            foreach (var image in images) // <-- Используем image из цикла
            {
                string fileName = await ProcessImageAsync(image); 
                savedUrls.Add($"/uploads/{fileName}");
            }
        }
        else
        {
            throw new ArgumentException("Загрузите хотя бы одно изображение.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            OldPrice = request.OldPrice,
            Description = request.Description ?? string.Empty,
            Composition = request.Composition ?? string.Empty,
            Weight = request.Weight ?? 0,
            Stock = request.Stock ?? 0,
            IsNew = request.IsNew,
            IsHit = request.IsHit,
            ImageUrls = savedUrls, 
            SKU = $"MISH-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Возвращаем DTO сразу после создания
        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Description = product.Description,
            ImageUrls = product.ImageUrls,
            MainImageUrl = product.ImageUrls.FirstOrDefault() ?? $"{_baseUrl}/uploads/placeholder.jpg",
            Rating = product.Rating,
            Stock = product.Stock,
            SKU = product.SKU,
            Weight = product.Weight,
            Composition = product.Composition
        };
    }

    public async Task<bool> UpdateAsync(int id, Product product)
    {
        if (id != product.Id) return false;
        _context.Entry(product).State = EntityState.Modified;
        try {
            await _context.SaveChangesAsync();
            return true;
        } catch (DbUpdateConcurrencyException) {
            if (!await _context.Products.AnyAsync(e => e.Id == id)) return false;
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    // Приватный метод для обработки фото, чтобы не засорять основной код
    private async Task<string> ProcessImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Главное изображение обязательно.");

        if (file.Length > 3 * 1024 * 1024) 
            throw new ArgumentException("Файл слишком большой. Максимум 3MB.");

        try 
        {
            using var image = Image.Load(file.OpenReadStream());
            string fileName = $"{Guid.NewGuid()}.jpg";
            string path = Path.Combine(_uploadsPath, fileName);
            
            var encoder = new JpegEncoder() { Quality = 80 };
            await image.SaveAsync(path, encoder);
            
            return fileName;
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("Неверный формат изображения.");
        }
    }
}