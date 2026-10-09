using System.ComponentModel.DataAnnotations;

namespace FromZeroToSeniorAI.Api.DTOs;

public sealed record CreateOrderItemDto(
    Guid ProductId,
    [Range(1, 100)] int Quantity);

public sealed record CreateOrderDto(
    [Required, MinLength(1)] List<CreateOrderItemDto> Items);

public sealed record UpdateOrderStatusDto(
    [Required] string Status);

public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public sealed record OrderDto(
    Guid Id,
    DateTimeOffset CreatedAtUtc,
    string Status,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items);
