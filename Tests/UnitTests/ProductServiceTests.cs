using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "GetProductById - Should return product when it exists")]
    public async Task GetProductById_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 9.99m,
            StockQuantity = 10,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
        result.Description.Should().Be("Test Description");
        result.CategoryId.Should().Be(categoryId);
        result.Price.Should().Be(9.99m);
        result.StockQuantity.Should().Be(10);
    }

    [Fact(DisplayName = "GetProductById - Should return null when product doesn't exist")]
    public async Task GetProductById_WithNonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductAsync(productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "CreateProduct - Should successfully create product when input is valid")]
    public async Task CreateProduct_WithValidInput_ReturnsNewProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CreateProductRequest(
            "New Product",
            "New Description",
            19.99m,
            5,
            categoryId
        );

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _service.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Description.Should().Be("New Description");
        result.CategoryId.Should().Be(categoryId);
        result.Price.Should().Be(19.99m);
        result.StockQuantity.Should().Be(5);

        _mockRepository.Verify(x => x.CreateAsync(
            It.Is<Product>(p => p.Name == request.Name 
                && p.Description == request.Description 
                && p.CategoryId == request.CategoryId
                && p.Price == request.Price 
                && p.StockQuantity == request.StockQuantity),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}