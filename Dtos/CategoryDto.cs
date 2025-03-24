using System;

namespace Backend.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);