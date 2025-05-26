using System;

namespace Backend.Models;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Pending";
    public DateTime PaymentDate { get; set; }
    public int? UserId { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}