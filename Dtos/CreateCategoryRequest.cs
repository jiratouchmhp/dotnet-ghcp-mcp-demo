namespace Backend.Dtos;

public record CreateCategoryRequest(
    string Name,
    string? Description
);