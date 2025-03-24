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

public class CategoryRepositoryTests
{
    private readonly Mock<ILogger<CategoryRepository>> _mockLogger;
    private readonly Mock<ILogger<AppDbContext>> _mockContextLogger;
    private readonly AppDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Setup loggers
        _mockLogger = new Mock<ILogger<CategoryRepository>>();
        _mockContextLogger = new Mock<ILogger<AppDbContext>>();
        
        // Create context and repository instances
        _context = new AppDbContext(options, _mockContextLogger.Object);
        _repository = new CategoryRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingCategory_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithExistingCategories_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Category 1", Description = "Description 1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Category 2", Description = "Description 2", CreatedAt = DateTime.UtcNow }
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public async Task CreateAsync_WithValidCategory_CreatesAndReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "New Category",
            Description = "New Description",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(category.Id);
        result.Name.Should().Be(category.Name);
        
        // Verify category was saved to database
        var savedCategory = await _context.Categories.FindAsync(result.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCategory_UpdatesAndReturnsCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Initial Name",
            Description = "Initial Description",
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Modify category
        category.Name = "Updated Name";
        category.Description = "Updated Description";
        category.UpdatedAt = DateTime.UtcNow;

        // Act
        var result = await _repository.UpdateAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        
        // Verify changes were saved to database
        var updatedCategory = await _context.Categories.FindAsync(category.Id);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.Should().BeEquivalentTo(category);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_DeletesAndReturnsTrue()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category to Delete",
            Description = "Will be deleted",
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(category.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify category was removed from database
        var deletedCategory = await _context.Categories.FindAsync(category.Id);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingCategory_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}