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
        _output.WriteLine("Starting CreateCategory test with valid data");
        
        // Arrange
        var request = new CreateCategoryRequest(
            "Test Category",
            "Test Description"
        );
        _output.WriteLine($"Request data: {JsonSerializer.Serialize(request)}");

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        _output.WriteLine("Sending POST request to /api/categories");
        var response = await _client.PostAsync("/api/categories", content);
        _output.WriteLine($"Received response with status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {responseContent}");
        
        var category = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        category.Should().NotBeNull();
        category!.Name.Should().Be(request.Name);
        category.Description.Should().Be(request.Description);
        _output.WriteLine($"Successfully created category with ID: {category.Id}");
    }

    [Fact(DisplayName = "GET /api/categories/{id} - Should return existing category")]
    public async Task GetCategory_WithExistingCategory_ReturnsCategory()
    {
        _output.WriteLine("Starting GetCategory test for existing category");
        
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Test Get Category",
            "Test Description"
        );
        _output.WriteLine($"Creating test category with data: {JsonSerializer.Serialize(createRequest)}");

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Created category response: {createResponseContent}");
        
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Act
        _output.WriteLine($"Fetching category with ID: {createdCategory!.Id}");
        var response = await _client.GetAsync($"/api/categories/{createdCategory.Id}");
        _output.WriteLine($"Get response status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Get response content: {responseContent}");
        
        var category = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        category.Should().NotBeNull();
        category!.Id.Should().Be(createdCategory.Id);
        category.Name.Should().Be(createRequest.Name);
        category.Description.Should().Be(createRequest.Description);
        _output.WriteLine("Category retrieval test completed successfully");
    }

    [Fact(DisplayName = "GET /api/categories/{id} - Should return 404 for non-existing category")]
    public async Task GetCategory_WithNonExistingId_ReturnsNotFound()
    {
        var nonExistingId = Guid.NewGuid();
        _output.WriteLine($"Testing GET request for non-existing category ID: {nonExistingId}");
        
        // Act
        var response = await _client.GetAsync($"/api/categories/{nonExistingId}");
        _output.WriteLine($"Received response with status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _output.WriteLine("Successfully verified 404 Not Found response");
    }

    [Fact(DisplayName = "PUT /api/categories/{id} - Should update existing category")]
    public async Task UpdateCategory_WithValidData_ReturnsUpdatedCategory()
    {
        _output.WriteLine("Starting UpdateCategory test with valid data");
        
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Initial Category",
            "Initial Description"
        );
        _output.WriteLine($"Creating initial category with data: {JsonSerializer.Serialize(createRequest)}");

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Created initial category response: {createResponseContent}");
        
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Update the category
        var updateRequest = new CreateCategoryRequest(
            "Updated Category",
            "Updated Description"
        );
        _output.WriteLine($"Updating category with data: {JsonSerializer.Serialize(updateRequest)}");

        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateRequest),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        _output.WriteLine($"Sending PUT request to update category with ID: {createdCategory!.Id}");
        var response = await _client.PutAsync($"/api/categories/{createdCategory.Id}", updateContent);
        _output.WriteLine($"Received response with status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Update response content: {responseContent}");
        
        var updatedCategory = JsonSerializer.Deserialize<CategoryDto>(responseContent, _jsonOptions);

        updatedCategory.Should().NotBeNull();
        updatedCategory!.Id.Should().Be(createdCategory.Id);
        updatedCategory.Name.Should().Be(updateRequest.Name);
        updatedCategory.Description.Should().Be(updateRequest.Description);
        _output.WriteLine("Category update test completed successfully");
    }

    [Fact(DisplayName = "DELETE /api/categories/{id} - Should delete existing category")]
    public async Task DeleteCategory_WithExistingCategory_ReturnsNoContent()
    {
        _output.WriteLine("Starting DeleteCategory test");
        
        // Arrange
        // First create a category
        var createRequest = new CreateCategoryRequest(
            "Category to Delete",
            "Will be deleted"
        );
        _output.WriteLine($"Creating test category with data: {JsonSerializer.Serialize(createRequest)}");

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/categories", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Created category response: {createResponseContent}");
        
        var createdCategory = JsonSerializer.Deserialize<CategoryDto>(createResponseContent, _jsonOptions);

        // Act
        _output.WriteLine($"Attempting to delete category with ID: {createdCategory!.Id}");
        var response = await _client.DeleteAsync($"/api/categories/{createdCategory.Id}");
        _output.WriteLine($"Delete response status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category no longer exists
        _output.WriteLine($"Verifying category {createdCategory.Id} no longer exists");
        var getResponse = await _client.GetAsync($"/api/categories/{createdCategory.Id}");
        _output.WriteLine($"Get response after delete status code: {getResponse.StatusCode}");
        
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _output.WriteLine("Delete test completed successfully");
    }
}