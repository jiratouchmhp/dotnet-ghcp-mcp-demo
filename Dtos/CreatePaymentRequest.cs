using System;

namespace Backend.Dtos;

public record CreatePaymentRequest(
    decimal Amount,
    string Currency = "USD",
    string Status = "Pending",
    DateTime? PaymentDate = null,
    int? UserId = null,
    string? TransactionId = null,
    string? PaymentMethod = null,
    string? Description = null
);