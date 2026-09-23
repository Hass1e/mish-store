using Microsoft.AspNetCore.Mvc;
using Mish.Api.Services;
using Mish.Api.Models;
using Mish.Api.Dtos;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    // Внедряем сервис вместо контекста БД
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

//////////////////////////////////////////////////////////////
    [HttpGet][ResponseCache(NoStore = true)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var result = await _productService.GetAllAsync(page, size);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        var dto = await _productService.GetByIdAsync(id);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

//////////////////////////////////////////////////////////////
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromForm] CreateProductRequest request, IFormFile[]? images)
    {
        try 
        {
            var dto = await _productService.CreateAsync(request, images);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

//////////////////////////////////////////////////////////////
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        var success = await _productService.UpdateAsync(id, product);
        if (!success) return BadRequest();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}