using FromZeroToSeniorAI.Api.Data;
using FromZeroToSeniorAI.Api.DTOs;
using FromZeroToSeniorAI.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FromZeroToSeniorAI.Api.Services;

public class CategoryService(AppDbContext db)
{
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(category.Id, category.Name))
            .ToListAsync(cancellationToken);

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new CategoryDto(category.Id, category.Name))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var normalizedName = dto.Name.Trim();
        var exists = await db.Categories.AnyAsync(
            category => category.Name == normalizedName,
            cancellationToken);

        if (exists)
        {
            throw new ArgumentException("A category with that name already exists.");
        }

        var category = new Category { Id = Guid.NewGuid(), Name = normalizedName };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<CategoryDto?> UpdateAsync(
        Guid id,
        UpdateCategoryDto dto,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return null;
        }

        var normalizedName = dto.Name.Trim();
        var duplicate = await db.Categories.AnyAsync(
            candidate => candidate.Id != id && candidate.Name == normalizedName,
            cancellationToken);

        if (duplicate)
        {
            throw new ArgumentException("A category with that name already exists.");
        }

        category.Name = normalizedName;
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FindAsync([id], cancellationToken);
        if (category is null)
        {
            return false;
        }

        var hasProducts = await db.Products.AnyAsync(
            product => product.CategoryId == id,
            cancellationToken);

        if (hasProducts)
        {
            throw new InvalidOperationException(
                "The category cannot be deleted while it contains products.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

