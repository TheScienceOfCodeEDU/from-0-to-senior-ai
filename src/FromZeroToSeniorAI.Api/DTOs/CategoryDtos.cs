using System.ComponentModel.DataAnnotations;

namespace FromZeroToSeniorAI.Api.DTOs;

public sealed record CreateCategoryDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name);

public sealed record UpdateCategoryDto(
    [Required, StringLength(100, MinimumLength = 2)] string Name);

public sealed record CategoryDto(Guid Id, string Name);
