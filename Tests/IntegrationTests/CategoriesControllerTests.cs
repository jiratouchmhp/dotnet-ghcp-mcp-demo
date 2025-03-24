using System.Net;
using System.Text;
using System.Text.Json;
using Backend.DbContext;
using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Backend.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Tests.IntegrationTests;

public class CategoriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public CategoriesControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureServices(services =>
            {
                // Remove the existing db context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestingDb");
                });

                // Register repositories and services
                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<CategoryService>();

                // Configure standard logging
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });
            });
        });

        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Ensure database is clean before each test
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        
        _output.WriteLine("Test context initialized with fresh in-memory database");
    }

    [Fact(DisplayName = "POST /api/categories - Should create new category with valid data")]
    public async Task CreateCategory_WithValidData_ReturnsCreatedCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest(
            "Test Category",
            "Test Description"
        );

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/categories", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        category.Should().NotBeNull();
        category!.Name.Should().Be(request.Name);
        category.Description.Should().Be(request.Description);
    }

    [Fact(DisplayName = "GET /api/categories/{id} - Should return existing category")]
    public async Task GetCategory_WithExistingCategory_ReturnsCategory()
    {
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Test Get Category",
            "Test Description"
        );

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/categories/{createdCategory!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var category = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        category.Should().NotBeNull();
        category!.Id.Should().Be(createdCategory.Id);
        category.Name.Should().Be(createRequest.Name);
        category.Description.Should().Be(createRequest.Description);
    }

    [Fact(DisplayName = "GET /api/categories/{id} - Should return 404 for non-existing category")]
    public async Task GetCategory_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/categories/{id} - Should update existing category")]
    public async Task UpdateCategory_WithValidData_ReturnsUpdatedCategory()
    {
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Initial Category",
            "Initial Description"
        );

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Update the category
        var updateRequest = new CreateCategoryRequest(
            "Updated Category",
            "Updated Description"
        );

        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PutAsync($"/api/categories/{createdCategory!.Id}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedCategory = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Id.Should().Be(createdCategory.Id);
        updatedCategory.Name.Should().Be(updateRequest.Name);
        updatedCategory.Description.Should().Be(updateRequest.Description);
    }

    [Fact(DisplayName = "DELETE /api/categories/{id} - Should delete existing category")]
    public async Task DeleteCategory_WithExistingCategory_ReturnsNoContent()
    {
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Category to Delete",
            "Will be deleted"
        );

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/categories/{createdCategory!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category no longer exists
        var getResponse = await _client.GetAsync($"/api/categories/{createdCategory.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}