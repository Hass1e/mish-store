using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Dtos;
using Mish.Api.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace Mish.Api.Controllers;

/// <summary>
/// Контроллер для работы с деревом категорий.
/// </summary>
/// <param name="categoriesService"></param>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public sealed class CategoriesController(ICategoriesService categoriesService) : ControllerBase
{
    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeDto>>> GetTree(
        CancellationToken cancellationToken)
    {
        var tree = await categoriesService.GetTreeAsync(cancellationToken);
        return Ok(tree);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryTreeDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await categoriesService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "products.edit_any")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await categoriesService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "products.edit_any")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(new { message = "ID в адресе не совпадает с ID категории." });
        }

        try
        {
            var updated = await categoriesService.UpdateAsync(request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("parents")]
    [Authorize(Policy = "products.edit_any")]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeDto>>> GetParents(
        CancellationToken cancellationToken)
    {
        var categories = await categoriesService.GetParentOptionsAsync(cancellationToken);
        return Ok(categories);
    }
}
