using Microsoft.EntityFrameworkCore;
using Mish.Api.Data;
using Mish.Api.DTOs.Cart;
using Mish.Api.Models;

namespace Mish.Api.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CartResponse> GetCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .Select(c => new CartItemResponse
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                Price = c.Product.Price,
                ImageUrl = c.Product.ImageUrls != null && c.Product.ImageUrls.Count > 0 
                    ? c.Product.ImageUrls[0] 
                    : null,
                Quantity = c.Quantity
            })
            .ToListAsync();

        return new CartResponse
        {
            Items = items,
            TotalItemsCount = items.Sum(i => i.Quantity),
            TotalPrice = items.Sum(i => i.Price * i.Quantity)
        };
    }

    public async Task<CartResponse> AddToCartAsync(int userId, AddToCartRequest request)
    {
        // Проверяем, существует ли товар
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new ArgumentException("Товар не найден");

        // Ищем, есть ли уже этот товар в корзине
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        await _context.SaveChangesAsync();
        
        // Возвращаем обновленную корзину
        return await GetCartAsync(userId);
    }

    public async Task RemoveFromCartAsync(int userId, int cartItemId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveByProductIdAsync(int userId, int productId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}