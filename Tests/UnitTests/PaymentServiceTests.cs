using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        var expectedPayments = new List<Payment>
        {
            new() { Id = 1, Amount = 100.50m, Currency = "USD", UserId = 1 },
            new() { Id = 2, Amount = 200.75m, Currency = "EUR", UserId = 2 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedPayments);

        // Act
        var result = await _service.GetAllPaymentsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedPayments);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        var expectedPayment = new Payment
        {
            Id = 1,
            Amount = 100.50m,
            Currency = "USD",
            UserId = 1,
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedPayment);

        // Act
        var result = await _service.GetPaymentByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(expectedPayment);
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.GetPaymentByIdAsync(999);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetPaymentsByUserIdAsync_ShouldReturnUserPayments()
    {
        // Arrange
        var userId = 1;
        var expectedPayments = new List<Payment>
        {
            new() { Id = 1, Amount = 100.50m, Currency = "USD", UserId = userId },
            new() { Id = 2, Amount = 150.25m, Currency = "USD", UserId = userId }
        };
        _mockRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(expectedPayments);

        // Act
        var result = await _service.GetPaymentsByUserIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedPayments);
        _mockRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_ShouldCreatePaymentWithCorrectValues()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 100.50m,
            Currency = "USD",
            UserId = 1,
            Status = "Pending"
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _service.CreatePaymentAsync(payment);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(100.50m);
        result.Currency.Should().Be("USD");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_WithoutPaymentDate_ShouldSetCurrentDate()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 100.50m,
            Currency = "USD",
            UserId = 1,
            Status = "Pending",
            PaymentDate = default
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _service.CreatePaymentAsync(payment);

        // Assert
        result.PaymentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdatePaymentAsync_WithValidId_ShouldUpdatePayment()
    {
        // Arrange
        var existingPayment = new Payment
        {
            Id = 1,
            Amount = 100.50m,
            Currency = "USD",
            UserId = 1,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var updatePayment = new Payment
        {
            Amount = 150.75m,
            Currency = "USD",
            UserId = 1,
            Status = "Completed"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPayment);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _service.UpdatePaymentAsync(1, updatePayment);

        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(150.75m);
        result.Status.Should().Be("Completed");
        result.CreatedAt.Should().Be(existingPayment.CreatedAt);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePaymentAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);
        var payment = new Payment
        {
            Amount = 150.75m,
            Currency = "USD",
            UserId = 1
        };

        // Act
        var result = await _service.UpdatePaymentAsync(999, payment);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task DeletePaymentAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeletePaymentAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeletePaymentAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _service.DeletePaymentAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(999), Times.Once);
    }
}