using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockRepository;
    private readonly Mock<ILogger<PaymentService>> _mockLogger;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _mockRepository = new Mock<IPaymentRepository>();
        _mockLogger = new Mock<ILogger<PaymentService>>();
        _service = new PaymentService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllPaymentsAsync_ShouldReturnAllPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 100.00m, Currency = "USD", Status = "Completed" },
            new() { Id = 2, Amount = 75.50m, Currency = "EUR", Status = "Pending" }
        };
        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(payments);

        // Act
        var result = await _service.GetAllPaymentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        var payment = new Payment
        {
            Id = 1,
            Amount = 100.00m,
            Currency = "USD",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        // Act
        var result = await _service.GetPaymentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Amount.Should().Be(100.00m);
        result.Currency.Should().Be("USD");
        _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.GetPaymentByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPaymentsByUserIdAsync_ShouldReturnUserPayments()
    {
        // Arrange
        var userId = 1;
        var payments = new List<Payment>
        {
            new() { Id = 1, Amount = 100.00m, Currency = "USD", Status = "Completed", UserId = userId },
            new() { Id = 2, Amount = 75.50m, Currency = "EUR", Status = "Pending", UserId = userId }
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(payments);

        // Act
        var result = await _service.GetPaymentsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCreateAndReturnPayment()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending",
            PaymentDate: DateTime.UtcNow,
            UserId: 1,
            TransactionId: "TXN-123",
            PaymentMethod: "Credit Card",
            Description: "Test payment"
        );

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => 
            {
                p.Id = 1;
                return p;
            });

        // Act
        var result = await _service.CreatePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Amount.Should().Be(request.Amount);
        result.Currency.Should().Be(request.Currency);
        result.Status.Should().Be(request.Status);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePaymentAsync_WithValidId_ShouldUpdateAndReturnPayment()
    {
        // Arrange
        var id = 1;
        var request = new CreatePaymentRequest(
            Amount: 120.00m,
            Currency: "USD",
            Status: "Completed",
            PaymentMethod: "Credit Card",
            Description: "Updated payment"
        );

        var existingPayment = new Payment
        {
            Id = id,
            Amount = 100.00m,
            Currency = "USD",
            Status = "Pending",
            PaymentDate = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existingPayment);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment p, CancellationToken _) => p);

        // Act
        var result = await _service.UpdatePaymentAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Amount.Should().Be(request.Amount);
        result.Status.Should().Be(request.Status);
        result.Description.Should().Be(request.Description);
        _mockRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePaymentAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var id = 999;
        var request = new CreatePaymentRequest(Amount: 120.00m);

        _mockRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.UpdatePaymentAsync(id, request);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePaymentAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        var id = 1;
        _mockRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.DeletePaymentAsync(id);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePaymentAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var id = 999;
        _mockRepository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.DeletePaymentAsync(id);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}