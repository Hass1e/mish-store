namespace Mish.Api.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Mish.Api.Data;
using Mish.Api.Dtos;
using Mish.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Mish.Api.Extensions;
using Microsoft.Data.Sqlite;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public ProductService(AppDbContext context, IWebHostEnvironment env, IConfiguration config)
    {
        _context = context;
        _uploadsPath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(_uploadsPath);
        _baseUrl = config["ApiSettings:BaseUrl"]
            ?? throw new InvalidOperationException("Ключ ApiSettings:BaseUrl отсутствует.");
    }

    // ✅ ОПТИМИЗИРОВАННЫЙ МЕТОД ДЛЯ КАТАЛОГА (без дублирования кода)
    public async Task<object> GetAllAsync(
        int page,
        int size,
        ClaimsPrincipal? user,
        int? categoryId = null,
        string? search = null,
        string? sort = null)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 50;

        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLowerInvariant();
            query = query.Where(p =>
                AppDbContext.UnicodeLower(p.Name).Contains(searchTerm) ||
                AppDbContext.UnicodeLower(p.SKU).Contains(searchTerm) ||
                AppDbContext.UnicodeLower(p.CategoryName).Contains(searchTerm) ||
                AppDbContext.UnicodeLower(p.Description).Contains(searchTerm));
        }

        if (_context.Database.IsSqlite() && !string.IsNullOrWhiteSpace(search))
        {
            await _context.Database.OpenConnectionAsync();
            try
            {
                var connection = (SqliteConnection)_context.Database.GetDbConnection();
                connection.CreateFunction<string?, string?>(
                    "unicode_lower",
                    value => value?.ToLowerInvariant());

                return await GetPagedProductsAsync(query, page, size, sort);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        return await GetPagedProductsAsync(query, page, size, sort);
    }

    // ПУБЛИЧНАЯ ДЕТАЛЬНАЯ СТРАНИЦА
    public async Task<ProductDetailDto?> GetByIdAsync(int id, ClaimsPrincipal? user)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return null;

        return MapToDetailDto(product);
    }

    // ProductService.cs

    public async Task<ProductDetailDto?> GetForEditAsync(int id, ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        var isAdmin = user.HasPermission("products.edit_any");

        // Загружаем товар И связи с продавцами
        var product = await _context.Products
            .Include(p => p.ProductSellers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        // 🔒 ЛОГИКА ПРАВ: Админ видит всё, остальные — только если они Owner
        bool isOwner = product.ProductSellers.Any(ps => 
            ps.SellerId == userId && ps.RoleInProduct == "Owner");

        if (!isAdmin && !isOwner) return null;

        
        // Возвращаем ПОЛНЫЙ DTO со всеми полями для формы
        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            OldPrice = product.OldPrice,
            Description = product.Description,
            Composition = product.Composition,
            Weight = product.Weight,
            Stock = product.Stock,
            SKU = product.SKU,
            IsNew = product.IsNew,
            IsHit = product.IsHit,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            // В маппинге DTO внутри GetForEditAsync:
            ImageUrls = product.ImageUrls?
                .Select(url => url.StartsWith("/uploads/") || url.StartsWith("http") 
                    ? url 
                    : $"/uploads/{url}")
                .ToList() ?? new List<string>(),

            MainImageUrl = product.ImageUrls?.FirstOrDefault() != null
                ? (product.ImageUrls.First().StartsWith("/uploads/") || product.ImageUrls.First().StartsWith("http")
                    ? product.ImageUrls.First()
                    : $"/uploads/{product.ImageUrls.First()}")
                : $"{_baseUrl}/uploads/placeholder.jpg",
            Rating = product.Rating,
            Tags = product.Tags ?? new List<string>()
        };
    }

    public async Task<object> GetManagedProductsAsync(int page, int size, ClaimsPrincipal user)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 50;

        var isAdmin = user.HasPermission("products.edit_any");
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!isAdmin)
        {
            if (TryGetUserId(user, out var userId))
            {
                query = query.Where(p => p.ProductSellers.Any(ps => ps.SellerId == userId));
            }
            else
            {
                query = query.Where(_ => false);
            }
        }

        var total = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ManagedProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Price,
                p.IsActive,
                p.UpdatedAt))
            .ToListAsync();

        return new
        {
            items = products,
            total,
            page,
            size,
            totalPages = (int)Math.Ceiling(total / (double)size)
        };
    }

    // СОЗДАНИЕ ТОВАРА
    public async Task<ProductDetailDto> CreateAsync(CreateProductRequest request, IFormFile[]? images, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 6 || request.Name.Length > 32)
            throw new ArgumentException("Название должно быть от 6 до 32 символов.");
            
        if (request.Price <= 0)
            throw new ArgumentException("Цена должна быть больше нуля.");

        var categoryName = string.Empty;
        if (request.CategoryId != 0)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Выбранная категория не найдена.");
            }

            categoryName = category.Name;
        }

        List<string> savedUrls = new();
        if (images != null && images.Length > 0)
        {
            foreach (var image in images)
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
            IsActive = request.IsActive,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            Tags = request.Tags ?? new List<string>(),
            ImageUrls = savedUrls, 
            SKU = $"MISH-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            // 🔗 АВТОМАТИЧЕСКОЕ НАЗНАЧЕНИЕ ВЛАДЕЛЬЦА
            ProductSellers = new List<ProductSeller>
            {
                new() { SellerId = userId, RoleInProduct = "Owner" }
            }
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDetailDto(product);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request, IFormFile[]? images, ClaimsPrincipal user)
    {
        var isAdmin = user.HasPermission("products.edit_any");
        var hasUserId = TryGetUserId(user, out var userId);
        if (!isAdmin && !hasUserId) return false;

        var product = await _context.Products
            .Include(p => p.ProductSellers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        var canEdit = isAdmin || (hasUserId && product.ProductSellers.Any(ps => ps.SellerId == userId));
        if (!canEdit) return false;

        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.OldPrice = request.OldPrice;
        product.Description = request.Description;
        product.Composition = request.Composition ?? string.Empty;
        product.Weight = request.Weight;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.IsNew = request.IsNew;
        product.IsHit = request.IsHit;
        product.CategoryId = request.CategoryId;
        product.Tags = request.Tags ?? product.Tags;
        product.UpdatedAt = DateTime.UtcNow;

        var finalImageUrls = new List<string>(request.ExistingImages ?? product.ImageUrls ?? new List<string>());

        if (images != null && images.Length > 0)
        {
            foreach (var image in images)
            {
                var fileName = await ProcessImageAsync(image);
                finalImageUrls.Add($"/uploads/{fileName}");
            }
        }

        product.ImageUrls = finalImageUrls;

        await _context.SaveChangesAsync();
        return true;
    }

    // УДАЛЕНИЕ ТОВАРА
    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal user)
    {
        var isAdmin = user.HasPermission("products.delete");
        var hasUserId = TryGetUserId(user, out var userId);
        if (!isAdmin && !hasUserId) return false;

        var product = await _context.Products
            .Include(p => p.ProductSellers)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        // 🔒 ПРОВЕРКА ПРАВ НА УДАЛЕНИЕ
        bool canDelete = isAdmin || (product.ProductSellers.Any(ps => ps.SellerId == userId) && user.HasPermission("products.delete"));
        if (!canDelete) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    // ✅ ПРИВАТНЫЙ МЕТОД: ЕДИНАЯ ЛОГИКА ПАГИНАЦИИ И ПРОЕКЦИИ
    private async Task<object> GetPagedProductsAsync(IQueryable<Product> query, int page, int size, string? sort = null)
    {
        var total = await query.CountAsync();

        var orderedQuery = sort switch
        {
            "price_asc" => query.OrderBy(p => (double)p.Price).ThenByDescending(p => p.CreatedAt),
            "price_desc" => query.OrderByDescending(p => (double)p.Price).ThenByDescending(p => p.CreatedAt),
            "name" => query.OrderBy(p => p.Name).ThenByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Rating).ThenByDescending(p => p.CreatedAt),
            "hit" => query.OrderByDescending(p => p.IsHit).ThenByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var products = await orderedQuery
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

        return new
        {
            items = products,
            total,
            page,
            size,
            totalPages = (int)Math.Ceiling(total / (double)size),
            hasMore = page * size < total
        };
    }

    // ✅ ПРИВАТНЫЙ МЕТОД: МАППИНГ В DETAIL DTO (чтобы не дублировать код)
    private ProductDetailDto MapToDetailDto(Product product)
    {
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
            Composition = product.Composition,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId
        };
    }

    // ОБРАБОТКА ИЗОБРАЖЕНИЙ
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
        catch (InvalidImageContentException)
        {
            throw new ArgumentException("Файл изображения повреждён или имеет неверный формат.");
        }
    }

    
}
