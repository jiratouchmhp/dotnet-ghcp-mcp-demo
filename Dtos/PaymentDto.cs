using System;

namespace Backend.Dtos;

public record PaymentDto(
    int Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PaymentDate,
    int? UserId,
    string? TransactionId,
    string? PaymentMethod,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);