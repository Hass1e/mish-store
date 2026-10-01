using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Dtos;
using Mish.Api.Extensions;
using Mish.Api.Services;
using Mish.Api.Models;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase // ← УБРАЛ [Authorize] С КЛАССА
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // ✅ ПУБЛИЧНЫЙ КАТАЛОГ: доступен всем (даже без токена)
    [HttpGet]
    [AllowAnonymous] // ← Явно разрешаем анонимный доступ
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null)
    {
        var user = User?.Identity?.IsAuthenticated == true ? User : null;
        var result = await _productService.GetAllAsync(page, size, user, categoryId, search, sort);
        return Ok(result);
    }

    // ✅ ДЕТАЛЬНАЯ СТРАНИЦА ТОВАРА: тоже публичная
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        var user = User?.Identity?.IsAuthenticated == true ? User : null;

        var dto = await _productService.GetByIdAsync(id, user);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    //  СОЗДАНИЕ: только для авторизованных с правом products.create
    [HttpPost]
    [Authorize] // ← Требуется авторизация
    [RequirePermission("products.create")]
    public async Task<ActionResult<ProductDetailDto>> Create([FromForm] CreateProductRequest request, IFormFile[]? images)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        try 
        {
            var dto = await _productService.CreateAsync(request, images, userId);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("managed")]
    [Authorize]
    public async Task<ActionResult> GetManaged([FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var result = await _productService.GetManagedProductsAsync(page, size, User);
        return Ok(result);
    }

    [HttpGet("{id:int}/edit")]
    [Authorize]
    public async Task<ActionResult<ProductDetailDto>> GetForEdit(int id)
    {
        var dto = await _productService.GetForEditAsync(id, User);
        
        if (dto == null) 
        {
            // Если товар есть, но прав нет — возвращаем 403, а не 404
            // (чтобы пользователь понимал: объект существует, но доступ запрещен)
            return Forbid(); 
        }
            
        return Ok(dto);
    }
    
    // 🔒 ОБНОВЛЕНИЕ: только для авторизованных
    [HttpPut("{id:int}")]
    [Authorize]
    [RequirePermission("products.edit_any")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request, IFormFile[]? images)
    {
        var success = await _productService.UpdateAsync(id, request, images, User);
        if (!success) return Forbid();
        return NoContent();
    }

    // 🔒 УДАЛЕНИЕ: только для авторизованных
    [HttpDelete("{id:int}")]
    [Authorize]
    [RequirePermission("products.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteAsync(id, User);
        if (!success) return Forbid();
        return NoContent();
    }
}