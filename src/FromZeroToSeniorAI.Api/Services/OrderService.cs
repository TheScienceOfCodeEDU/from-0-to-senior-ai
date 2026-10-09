using FromZeroToSeniorAI.Api.Data;
using FromZeroToSeniorAI.Api.DTOs;
using FromZeroToSeniorAI.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FromZeroToSeniorAI.Api.Services;

public class OrderService(AppDbContext db)
{
    private static readonly HashSet<string> ValidStatuses =
        ["Pending", "Preparing", "Ready", "Delivered", "Cancelled"];

    public async Task<List<OrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await OrderQuery()
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await OrderQuery()
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<OrderDto> CreateAsync(
        CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var requestedItems = dto.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        var productIds = requestedItems.Select(item => item.ProductId).ToList();
        var products = await db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new ArgumentException("One or more selected products do not exist.");
        }

        if (products.Values.Any(product => !product.IsAvailable))
        {
            throw new ArgumentException("One or more selected products are unavailable.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Status = "Pending"
        };

        foreach (var requestedItem in requestedItems)
        {
            var product = products[requestedItem.ProductId];
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = requestedItem.Quantity,
                UnitPrice = product.Price
            });
        }

        order.Total = order.Items.Sum(item => item.Quantity * item.UnitPrice);
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(order.Id, cancellationToken))!;
    }

    public async Task<OrderDto?> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusDto dto,
        CancellationToken cancellationToken)
    {
        var normalizedStatus = dto.Status.Trim();
        var canonicalStatus = ValidStatuses.SingleOrDefault(
            status => status.Equals(normalizedStatus, StringComparison.OrdinalIgnoreCase));

        if (canonicalStatus is null)
        {
            throw new ArgumentException(
                $"Status must be one of: {string.Join(", ", ValidStatuses)}.");
        }

        var order = await db.Orders.FindAsync([id], cancellationToken);
        if (order is null)
        {
            return null;
        }

        order.Status = canonicalStatus;
        await db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FindAsync([id], cancellationToken);
        if (order is null)
        {
            return false;
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<Order> OrderQuery() =>
        db.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .AsSplitQuery();

    private static OrderDto Map(Order order) =>
        new(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.Total,
            order.Items.Select(item => new OrderItemDto(
                item.ProductId,
                item.Product!.Name,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice)).ToList());
}

