using System.Diagnostics;
using Backend.DbContext;
using Backend.Models;
using Backend.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.UnitTests;

public class ProductRepositoryTests
{
    private readonly Mock<ILogger<ProductRepository>> _mockLogger;
    private readonly Mock<ILogger<AppDbContext>> _mockContextLogger;
    private readonly AppDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Setup loggers
        _mockLogger = new Mock<ILogger<ProductRepository>>();
        _mockContextLogger = new Mock<ILogger<AppDbContext>>();
        
        // Create context and repository instances
        _context = new AppDbContext(options, _mockContextLogger.Object);
        _repository = new ProductRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            StockQuantity = 100,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Price.Should().Be(product.Price);
        result.StockQuantity.Should().Be(product.StockQuantity);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingProduct_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithExistingProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Description = "Description 1",
                Price = 19.99m,
                StockQuantity = 50,
                CreatedAt = DateTime.UtcNow
            },
            new() {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Description = "Description 2",
                Price = 29.99m,
                StockQuantity = 100,
                CreatedAt = DateTime.UtcNow
            }
        };
        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task CreateAsync_WithValidProduct_CreatesAndReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Description = "New Description",
            Price = 39.99m,
            StockQuantity = 75,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(product.Id);
        result.Name.Should().Be(product.Name);
        
        // Verify product was saved to database
        var savedProduct = await _context.Products.FindAsync(result.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_UpdatesAndReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Original Product",
            Description = "Original Description",
            Price = 49.99m,
            StockQuantity = 25,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var updatedProduct = new Product
        {
            Id = product.Id,
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 59.99m,
            StockQuantity = 30,
            CreatedAt = product.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.UpdateAsync(updatedProduct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProduct);
        
        // Verify product was updated in database
        var savedProduct = await _context.Products.FindAsync(product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Should().BeEquivalentTo(updatedProduct);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_DeletesAndReturnsTrue()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Product to Delete",
            Description = "Will be deleted",
            Price = 9.99m,
            StockQuantity = 10,
            CreatedAt = DateTime.UtcNow
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(product.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify product was deleted from database
        var deletedProduct = await _context.Products.FindAsync(product.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingProduct_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistingId);

        // Assert
        result.Should().BeFalse();
    }
}