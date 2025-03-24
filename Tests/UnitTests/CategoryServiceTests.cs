using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.UnitTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<CategoryService>>();
        _service = new CategoryService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact(DisplayName = "GetCategoryById - Should return category when it exists")]
    public async Task GetCategoryById_WithExistingCategory_ReturnsCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category 
        { 
            Id = categoryId, 
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetCategoryAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
    }

    [Fact(DisplayName = "GetCategoryById - Should return null when category doesn't exist")]
    public async Task GetCategoryById_WithNonExistingCategory_ReturnsNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.GetCategoryAsync(categoryId);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "CreateCategory - Should successfully create category when input is valid")]
    public async Task CreateCategory_WithValidInput_ReturnsNewCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest("Test Category", "Test Description");
        Category? savedCategory = null;

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Category>(), default))
            .Callback<Category, CancellationToken>((c, _) => savedCategory = c)
            .ReturnsAsync((Category c, CancellationToken _) => c);

        // Act
        var result = await _service.CreateCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be(request.Name);
        savedCategory.Description.Should().Be(request.Description);
    }

    [Fact(DisplayName = "UpdateCategory - Should update existing category with valid input")]
    public async Task UpdateCategory_WithValidInput_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Old Name",
            Description = "Old Description",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var request = new CreateCategoryRequest("New Name", "New Description");

        _mockRepository.Setup(r => r.GetByIdAsync(categoryId, default))
            .ReturnsAsync(existingCategory);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Category>(), default))
            .ReturnsAsync((Category c, CancellationToken _) => c);

        // Act
        var result = await _service.UpdateCategoryAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
    }

    [Fact(DisplayName = "DeleteCategory - Should return true when category exists")]
    public async Task DeleteCategory_WithExistingCategory_ReturnsTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(categoryId, default))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "DeleteCategory - Should return false when category doesn't exist")]
    public async Task DeleteCategory_WithNonExistingCategory_ReturnsFalse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(categoryId, default))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeFalse();
    }
}