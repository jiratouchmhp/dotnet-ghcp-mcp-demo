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

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _service = new CustomerService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllCustomersAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var expectedCustomers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedCustomers);

        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedCustomers);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithUniqueEmail_ShouldCreateCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        _mockRepository.Setup(r => r.GetByEmailAsync(customer.Email)).ReturnsAsync((Customer?)null);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _service.CreateCustomerAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithDuplicateEmail_ShouldReturnNull()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        var newCustomer = new Customer
        {
            FirstName = "Johnny",
            LastName = "Doe",
            Email = "john@example.com"
        };
        _mockRepository.Setup(r => r.GetByEmailAsync(newCustomer.Email)).ReturnsAsync(existingCustomer);

        // Act
        var result = await _service.CreateCustomerAsync(newCustomer);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithValidId_ShouldUpdateCustomer()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var updateCustomer = new Customer
        {
            FirstName = "Johnny",
            LastName = "Doe",
            Email = "john@example.com"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCustomer);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _service.UpdateCustomerAsync(1, updateCustomer);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Johnny");
        result.CreatedAt.Should().Be(existingCustomer.CreatedAt);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteCustomerAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}