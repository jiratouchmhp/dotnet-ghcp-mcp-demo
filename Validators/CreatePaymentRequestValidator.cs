using System.Collections.Generic;
using Backend.Dtos;
using FluentValidation;

namespace Backend.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    private static readonly HashSet<string> ValidCurrencies = new(new[] 
    { 
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY" 
    });

    private static readonly HashSet<string> ValidStatuses = new(new[] 
    { 
        "Pending", "Completed", "Failed", "Refunded", "Cancelled" 
    });

    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Must(BeValidCurrency)
            .WithMessage("Currency must be a valid 3-letter ISO currency code (e.g., USD, EUR, GBP)");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(BeValidStatus)
            .WithMessage("Status must be one of: Pending, Completed, Failed, Refunded, or Cancelled");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be provided and greater than zero");

        RuleFor(x => x.TransactionReference)
            .MaximumLength(100)
            .WithMessage("Transaction reference cannot exceed 100 characters");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50)
            .WithMessage("Payment method cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }

    private bool BeValidCurrency(string? currency)
    {
        if (string.IsNullOrEmpty(currency))
            return false;
        
        return ValidCurrencies.Contains(currency);
    }

    private bool BeValidStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return false;
        
        return ValidStatuses.Contains(status);
    }
}