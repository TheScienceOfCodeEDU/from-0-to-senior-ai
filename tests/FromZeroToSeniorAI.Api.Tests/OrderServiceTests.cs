using FromZeroToSeniorAI.Api.Data;
using FromZeroToSeniorAI.Api.DTOs;
using FromZeroToSeniorAI.Api.Entities;
using FromZeroToSeniorAI.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FromZeroToSeniorAI.Api.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateAsync_CalculatesTotalAndStoresCurrentPrice()
    {
        await using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Comida" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Arepa",
            Price = 8.25m,
            IsAvailable = true,
            CategoryId = category.Id,
            Category = category
        };
        db.AddRange(category, product);
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        var result = await service.CreateAsync(
            new CreateOrderDto([new CreateOrderItemDto(product.Id, 3)]),
            CancellationToken.None);

        Assert.Equal(24.75m, result.Total);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(8.25m, Assert.Single(result.Items).UnitPrice);
    }

    [Fact]
    public async Task CreateAsync_RejectsUnavailableProduct()
    {
        await using var db = CreateDbContext();
        var category = new Category { Id = Guid.NewGuid(), Name = "Comida" };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Agotado",
            Price = 1m,
            IsAvailable = false,
            CategoryId = category.Id,
            Category = category
        };
        db.AddRange(category, product);
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        var action = () => service.CreateAsync(
            new CreateOrderDto([new CreateOrderItemDto(product.Id, 1)]),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
