using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DbContext;
using Backend.Models;
using Backend.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.UnitTests;

public class PaymentRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly PaymentRepository _repository;
    private readonly Mock<ILogger<PaymentRepository>> _mockLogger;

    public PaymentRepositoryTests()
    {
        // Set up in-memory database for testing
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PaymentDb_{Guid.NewGuid()}")
            .Options;
        
        _mockLogger = new Mock<ILogger<PaymentRepository>>();
        _context = new AppDbContext(options, Mock.Of<ILogger<AppDbContext>>());
        _repository = new PaymentRepository(_context, _mockLogger.Object);
        
        // Seed some test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var payments = new List<Payment>
        {
            new Payment
            {
                Id = 1,
                Amount = 100.00m,
                Currency = "USD",
                Status = "Completed",
                PaymentDate = DateTime.UtcNow.AddDays(-1),
                UserId = 1,
                TransactionId = "TXN-123",
                PaymentMethod = "Credit Card",
                Description = "Test payment 1",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Payment
            {
                Id = 2,
                Amount = 75.50m,
                Currency = "EUR",
                Status = "Pending",
                PaymentDate = DateTime.UtcNow,
                UserId = 1,
                TransactionId = "TXN-456",
                PaymentMethod = "PayPal",
                Description = "Test payment 2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Payment
            {
                Id = 3,
                Amount = 50.25m,
                Currency = "USD",
                Status = "Completed",
                PaymentDate = DateTime.UtcNow.AddDays(-2),
                UserId = 2,
                TransactionId = "TXN-789",
                PaymentMethod = "Bank Transfer",
                Description = "Test payment 3",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
        
        _context.Payments.AddRange(payments);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPayments()
    {
        // Act
        var result = await _repository.GetAllAsync();
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnPayment()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Amount.Should().Be(100.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserPayments()
    {
        // Act
        var result = await _repository.GetByUserIdAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.UserId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddNewPayment()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 200.00m,
            Currency = "USD",
            Status = "Pending",
            PaymentDate = DateTime.UtcNow,
            UserId = 3,
            TransactionId = "TXN-NEW",
            PaymentMethod = "Credit Card",
            Description = "New test payment",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Act
        var result = await _repository.CreateAsync(payment);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        // Verify it was added to the database
        var savedPayment = await _context.Payments.FindAsync(result.Id);
        savedPayment.Should().NotBeNull();
        savedPayment!.Amount.Should().Be(200.00m);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdatePayment()
    {
        // Arrange
        var payment = await _context.Payments.FindAsync(2);
        payment!.Amount = 80.00m;
        payment.Status = "Completed";
        
        // Act
        var result = await _repository.UpdateAsync(payment);
        
        // Assert
        result.Should().NotBeNull();
        result!.Amount.Should().Be(80.00m);
        result.Status.Should().Be("Completed");
        
        // Verify it was updated in the database
        var updatedPayment = await _context.Payments.FindAsync(2);
        updatedPayment!.Amount.Should().Be(80.00m);
        updatedPayment.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var payment = new Payment { Id = 999 };
        
        // Act
        var result = await _repository.UpdateAsync(payment);
        
        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Act
        var result = await _repository.DeleteAsync(3);
        
        // Assert
        result.Should().BeTrue();
        
        // Verify it was deleted from the database
        var deletedPayment = await _context.Payments.FindAsync(3);
        deletedPayment.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);
        
        // Assert
        result.Should().BeFalse();
    }
}