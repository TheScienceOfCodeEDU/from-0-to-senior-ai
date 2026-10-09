using FromZeroToSeniorAI.Api.Data;
using FromZeroToSeniorAI.Api.DTOs;
using FromZeroToSeniorAI.Api.Entities;
using FromZeroToSeniorAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FromZeroToSeniorAI.Api.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsAndReturnsProductWithCategory()
    {
        await using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Bebidas" };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        var service = new ProductService(db);

        var result = await service.CreateAsync(
            new CreateProductDto("Café", "Filtrado", 4.50m, true, category.Id),
            CancellationToken.None);

        Assert.Equal("Café", result.Name);
        Assert.Equal("Bebidas", result.CategoryName);
        Assert.Equal(4.50m, result.Price);
        Assert.Equal(1, await db.Products.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownCategory()
    {
        await using var db = CreateDbContext();
        var service = new ProductService(db);

        var action = () => service.CreateAsync(
            new CreateProductDto("Café", null, 4.50m, true, Guid.NewGuid()),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Contains("category", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
