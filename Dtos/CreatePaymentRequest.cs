using System;

namespace Backend.Dtos;

/// <summary>
/// Request data transfer object for creating a new payment
/// </summary>
public record CreatePaymentRequest(
    decimal Amount,
    string? Currency = "USD",
    string? Status = "Pending",
    DateTime? PaymentDate = null,
    int UserId = 0,
    string? TransactionReference = null,
    string? PaymentMethod = null,
    string? Description = null
);