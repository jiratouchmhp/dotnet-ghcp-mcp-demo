using System;
using Backend.Dtos;
using Backend.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Backend.Tests.UnitTests;

public class CreatePaymentRequestValidatorTests
{
    private readonly CreatePaymentRequestValidator _validator;

    public CreatePaymentRequestValidatorTests()
    {
        _validator = new CreatePaymentRequestValidator();
    }

    [Fact]
    public void Amount_WhenZeroOrNegative_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(0);
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Amount);
        
        // Test negative value
        request = new CreatePaymentRequest(-10.00m);
        
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Amount);
    }

    [Fact]
    public void Amount_WhenPositive_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m);
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Amount);
    }

    [Fact]
    public void Currency_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m, "");
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Currency);
    }

    [Fact]
    public void Currency_WhenNotThreeChars_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m, "US");
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Currency);
        
        // Test longer than 3 chars
        request = new CreatePaymentRequest(100.00m, "USDD");
        
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Currency);
    }

    [Fact]
    public void Currency_WhenThreeChars_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m, "USD");
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Currency);
        
        // Test different currency
        request = new CreatePaymentRequest(100.00m, "EUR");
        
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Currency);
    }

    [Fact]
    public void Status_WhenEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m, "USD", "");
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Status);
    }

    [Fact]
    public void Status_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longStatus = new string('x', 51);
        var request = new CreatePaymentRequest(100.00m, "USD", longStatus);
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Status);
    }

    [Fact]
    public void Status_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(100.00m, "USD", "Pending");
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Status);
        
        // Test different status
        request = new CreatePaymentRequest(100.00m, "USD", "Completed");
        
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Status);
    }

    [Fact]
    public void PaymentDate_WhenFarFuture_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            PaymentDate: DateTime.UtcNow.AddDays(10)
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.PaymentDate);
    }

    [Fact]
    public void PaymentDate_WhenPastOrNearFuture_ShouldNotHaveValidationError()
    {
        // Arrange - past date
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            PaymentDate: DateTime.UtcNow.AddDays(-1)
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.PaymentDate);
        
        // Test for near future (within allowed range)
        request = new CreatePaymentRequest(
            Amount: 100.00m,
            PaymentDate: DateTime.UtcNow.AddHours(12)
        );
        
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.PaymentDate);
    }

    [Fact]
    public void TransactionId_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longTransactionId = new string('x', 101);
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            TransactionId: longTransactionId
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.TransactionId);
    }

    [Fact]
    public void TransactionId_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            TransactionId: "TXN-123456789"
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.TransactionId);
    }

    [Fact]
    public void PaymentMethod_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longMethod = new string('x', 51);
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            PaymentMethod: longMethod
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.PaymentMethod);
    }

    [Fact]
    public void PaymentMethod_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            PaymentMethod: "Credit Card"
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.PaymentMethod);
    }

    [Fact]
    public void Description_WhenTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('x', 501);
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            Description: longDescription
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Description_WhenValid_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            Description: "Test payment description"
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void ValidRequest_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending",
            PaymentDate: DateTime.UtcNow,
            UserId: 1,
            TransactionId: "TXN-123456",
            PaymentMethod: "Credit Card",
            Description: "Test payment"
        );
        
        // Act & Assert
        _validator.TestValidate(request)
            .ShouldNotHaveAnyValidationErrors();
    }
}