using System;

namespace Backend.Dtos;

/// <summary>
/// Data transfer object for Payment entity
/// </summary>
public record PaymentDto(
    int Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PaymentDate,
    int UserId,
    string? TransactionReference,
    string? PaymentMethod,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt
);