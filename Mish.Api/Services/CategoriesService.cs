using Microsoft.EntityFrameworkCore;
using Mish.Api.Data;
using Mish.Api.Dtos;

namespace Mish.Api.Services;

public interface ICategoriesService
{
    Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken);
}

public sealed class CategoriesService(AppDbContext db) : ICategoriesService
{
    public async Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(
        CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var categoriesByParent = categories.ToLookup(category => category.ParentId);

        List<CategoryTreeDto> BuildTree(int? parentId)
        {
            return categoriesByParent[parentId]
                .Select(category => new CategoryTreeDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    Children = BuildTree(category.Id)
                })
                .ToList();
        }

        return BuildTree(null);
    }
}
