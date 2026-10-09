using FromZeroToSeniorAI.Api.Data;
using FromZeroToSeniorAI.Api.DTOs;
using FromZeroToSeniorAI.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FromZeroToSeniorAI.Api.Services;

public class ProductService(AppDbContext db)
{
    public async Task<List<ProductDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await ToDtos(db.Products.AsNoTracking().OrderBy(product => product.Name))
            .ToListAsync(cancellationToken);

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await ToDtos(db.Products.AsNoTracking().Where(product => product.Id == id))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ProductDto> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        await EnsureCategoryExistsAsync(dto.CategoryId, cancellationToken);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            CategoryId = dto.CategoryId
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(product.Id, cancellationToken))!;
    }

    public async Task<ProductDto?> UpdateAsync(
        Guid id,
        UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return null;
        }

        await EnsureCategoryExistsAsync(dto.CategoryId, cancellationToken);

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.IsAvailable = dto.IsAvailable;
        product.CategoryId = dto.CategoryId;

        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(product.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null)
        {
            return false;
        }

        var appearsInOrders = await db.OrderItems.AnyAsync(
            item => item.ProductId == id,
            cancellationToken);

        if (appearsInOrders)
        {
            throw new InvalidOperationException(
                "The product cannot be deleted because it appears in an order.");
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IQueryable<ProductDto> ToDtos(IQueryable<Product> products) =>
        products
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.IsAvailable,
                product.CategoryId,
                product.Category!.Name));

    private async Task EnsureCategoryExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        if (!await db.Categories.AnyAsync(
                category => category.Id == categoryId,
                cancellationToken))
        {
            throw new ArgumentException("The selected category does not exist.");
        }
    }
}
