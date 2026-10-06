using Microsoft.EntityFrameworkCore;
using Mish.Api.Data;
using Mish.Api.Dtos;
using Mish.Api.Models;

namespace Mish.Api.Services;

public interface ICategoriesService
{
    Task<IReadOnlyList<CategoryTreeDto>> GetTreeAsync(CancellationToken cancellationToken);
    Task<CategoryTreeDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryTreeDto>> GetParentOptionsAsync(CancellationToken cancellationToken);
    Task<CategoryTreeDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken);
    Task<bool> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken);
}

public sealed class CategoriesService(AppDbContext db) : ICategoriesService
{
    private const int MaxSlugLength = 50;
    private const int CategoryBlockSize = 1000;

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

    public async Task<CategoryTreeDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new CategoryTreeDto
            {
                Id = item.Id,
                Name = item.Name,
                Slug = item.Slug
            })
            .SingleOrDefaultAsync(cancellationToken);

        return category;
    }

    public async Task<IReadOnlyList<CategoryTreeDto>> GetParentOptionsAsync(
        CancellationToken cancellationToken)
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryTreeDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryTreeDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeName(request.Name);
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        try
        {
            if (db.Database.IsNpgsql())
            {
                transaction = await db.Database.BeginTransactionAsync(cancellationToken);
                await db.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(728194632)",
                    cancellationToken);
            }

            var categories = await db.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var categoryId = AllocateCategoryId(categories, request.ParentId);
            var slug = NormalizeSlug(request.Slug);
            await EnsureSlugIsAvailableAsync(slug, excludedId: null, cancellationToken);

            var category = new Category
            {
                Id = categoryId,
                Name = name,
                Slug = slug,
                ParentId = request.ParentId
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync(cancellationToken);

            if (db.Database.IsNpgsql())
            {
                await db.Database.ExecuteSqlRawAsync(
                    """
                    SELECT setval(
                        pg_get_serial_sequence('"Categories"', 'Id'),
                        GREATEST(
                            (SELECT COALESCE(MAX("Id"), 1) FROM "Categories"),
                            COALESCE(
                                pg_sequence_last_value(pg_get_serial_sequence('"Categories"', 'Id')),
                                1)),
                        true)
                    """,
                    cancellationToken);
            }

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return new CategoryTreeDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            };
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    public async Task<bool> UpdateAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        var name = NormalizeName(request.Name);
        var slug = NormalizeSlug(request.Slug);
        var categories = await db.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        if (categories.Any(item =>
                item.Id != category.Id
                && string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Категория с таким slug уже существует.");
        }

        var categoriesById = categories.ToDictionary(item => item.Id);
        var targetRootId = request.ParentId is int parentId
            ? FindRootId(parentId, categoriesById)
            : category.Id;

        if (!IsBlockRootId(targetRootId)
            || GetBlockRootId(category.Id) != targetRootId)
        {
            throw new ArgumentException(
                "Категорию нельзя переместить в другой диапазон ID.");
        }

        await EnsureParentIsNotDescendantAsync(
            category.Id,
            request.ParentId,
            cancellationToken);

        category.Name = name;
        category.Slug = slug;
        category.ParentId = request.ParentId;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static int AllocateCategoryId(IReadOnlyList<Category> categories, int? parentId)
    {
        var categoryById = categories.ToDictionary(category => category.Id);
        var occupiedIds = categoryById.Keys.ToHashSet();

        if (parentId is null)
        {
            for (long rootId = 1; rootId <= int.MaxValue - CategoryBlockSize + 1L;
                 rootId += CategoryBlockSize)
            {
                var blockEnd = rootId + CategoryBlockSize - 1;
                if (!occupiedIds.Any(id => id >= rootId && id <= blockEnd))
                {
                    return (int)rootId;
                }
            }

            throw new ArgumentException("Не осталось свободных диапазонов для корневых категорий.");
        }

        var rootCategoryId = FindRootId(parentId.Value, categoryById);
        var firstChildId = (long)rootCategoryId + 1;
        var lastChildId = (long)rootCategoryId + CategoryBlockSize - 1;

        var nextChildId = occupiedIds
            .Where(id => id >= firstChildId && id <= lastChildId)
            .Select(id => (long)id)
            .DefaultIfEmpty(rootCategoryId)
            .Max() + 1;

        if (nextChildId <= lastChildId)
        {
            return (int)nextChildId;
        }

        throw new ArgumentException("В диапазоне этой категории больше нет свободных ID.");
    }

    private static int FindRootId(int categoryId, IReadOnlyDictionary<int, Category> categoriesById)
    {
        var currentId = categoryId;
        var visited = new HashSet<int>();

        while (true)
        {
            if (!visited.Add(currentId) || !categoriesById.TryGetValue(currentId, out var category))
            {
                throw new ArgumentException(
                    "Не удалось определить корневую категорию для указанного родителя.");
            }

            if (category.ParentId is not int parentId)
            {
                if (!IsBlockRootId(category.Id))
                {
                    throw new ArgumentException(
                        "Корневая категория находится вне допустимого диапазона ID.");
                }

                return category.Id;
            }

            currentId = parentId;
        }
    }

    private static bool IsBlockRootId(int id)
    {
        return id > 0 && (id - 1) % CategoryBlockSize == 0;
    }

    private static int GetBlockRootId(int id)
    {
        return ((id - 1) / CategoryBlockSize * CategoryBlockSize) + 1;
    }

    private async Task EnsureParentIsNotDescendantAsync(
        int categoryId,
        int? parentId,
        CancellationToken cancellationToken)
    {
        if (parentId is null)
        {
            return;
        }

        var parentsById = await db.Categories
            .AsNoTracking()
            .Select(category => new { category.Id, category.ParentId })
            .ToDictionaryAsync(category => category.Id, category => category.ParentId, cancellationToken);

        var currentId = parentId;
        var visited = new HashSet<int>();
        while (currentId is int id)
        {
            if (id == categoryId || !visited.Add(id))
            {
                throw new ArgumentException(
                    "Нельзя переместить категорию внутрь неё самой или её подкатегории.");
            }

            currentId = parentsById.GetValueOrDefault(id);
        }
    }

    private async Task EnsureSlugIsAvailableAsync(
        string slug,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        if (await db.Categories.AnyAsync(
                category => category.Slug.ToLower() == slug
                    && (!excludedId.HasValue || category.Id != excludedId.Value),
                cancellationToken))
        {
            throw new ArgumentException("Категория с таким slug уже существует.");
        }
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("Название категории не может быть пустым.");
        }

        return normalized;
    }

    private static string NormalizeSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        if (slug.Length == 0
            || slug.Length > MaxSlugLength
            || !System.Text.RegularExpressions.Regex.IsMatch(
                slug,
                @"^[\p{L}\p{Nd}]+(?:-[\p{L}\p{Nd}]+)*$"))
        {
            throw new ArgumentException(
                "Slug обязателен, должен быть не длиннее 50 символов и содержать только буквы, цифры и одиночные дефисы между ними.");
        }

        return slug;
    }
}
