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
}
