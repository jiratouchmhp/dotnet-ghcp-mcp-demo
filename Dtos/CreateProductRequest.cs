namespace Backend.Dtos;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity
);