using Backend.Dtos;
using FluentValidation;

namespace Backend.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Amount is required and must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency code must be 3 characters (e.g., USD, EUR)");

        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Status is required and must not exceed 50 characters");

        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.PaymentDate.HasValue)
            .WithMessage("Payment date cannot be in the future (except for scheduled payments within 24 hours)");

        RuleFor(x => x.TransactionId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.TransactionId))
            .WithMessage("Transaction ID must not exceed 100 characters");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod))
            .WithMessage("Payment method must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters");
    }
}