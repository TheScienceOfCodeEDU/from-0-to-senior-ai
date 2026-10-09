using System.ComponentModel.DataAnnotations;

namespace FromZeroToSeniorAI.Api.DTOs;

public sealed record CreateProductDto(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description,
    [Range(0.01, 99999999)] decimal Price,
    bool IsAvailable,
    Guid CategoryId);

public sealed record UpdateProductDto(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [StringLength(500)] string? Description,
    [Range(0.01, 99999999)] decimal Price,
    bool IsAvailable,
    Guid CategoryId);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsAvailable,
    Guid CategoryId,
    string CategoryName);
