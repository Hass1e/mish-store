using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mish.Api.Dtos;
using Mish.Api.Services;

namespace Mish.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
