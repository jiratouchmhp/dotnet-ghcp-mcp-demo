using System.Net;
using System.Text;
using System.Text.Json;
using Backend.DbContext;
using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Tests.IntegrationTests;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;
    private readonly Guid _testCategoryId;

    public ProductsControllerTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
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

        // Create a test category for products
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category",
            Description = "Test Category Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        dbContext.Categories.Add(category);
        dbContext.SaveChanges();
        
        _testCategoryId = category.Id;
        _output.WriteLine($"Test category created with ID: {_testCategoryId}");
    }

    [Fact(DisplayName = "POST /api/products - Should create new product with valid data")]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        _output.WriteLine("Starting CreateProduct test with valid data");
        
        // Arrange
        var request = new CreateProductRequest(
            "Integration Test Product",
            "Test Description",
            29.99m,
            10,
            _testCategoryId
        );
        _output.WriteLine($"Request data: {JsonSerializer.Serialize(request)}");

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        _output.WriteLine("Sending POST request to /api/products");
        var response = await _client.PostAsync("/api/products", content);
        _output.WriteLine($"Received response with status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response content: {responseContent}");
        
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, _jsonOptions);

        product.Should().NotBeNull();
        product!.Name.Should().Be(request.Name);
        product.Description.Should().Be(request.Description);
        product.CategoryId.Should().Be(request.CategoryId);
        product.Price.Should().Be(request.Price);
        product.StockQuantity.Should().Be(request.StockQuantity);
        _output.WriteLine($"Successfully created product with ID: {product.Id}");
    }

    [Fact(DisplayName = "GET /api/products/{id} - Should return existing product")]
    public async Task GetProduct_WithExistingProduct_ReturnsProduct()
    {
        _output.WriteLine("Starting GetProduct test for existing product");
        
        // Arrange
        // First create a product
        var createRequest = new CreateProductRequest(
            "Test Get Product",
            "Test Description",
            39.99m,
            50,
            _testCategoryId
        );
        _output.WriteLine($"Creating test product with data: {JsonSerializer.Serialize(createRequest)}");

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/products", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Created product response: {createResponseContent}");
        
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(createResponseContent, _jsonOptions);

        // Act
        _output.WriteLine($"Fetching product with ID: {createdProduct!.Id}");
        var response = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        _output.WriteLine($"Get response status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Get response content: {responseContent}");
        
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, _jsonOptions);

        product.Should().NotBeNull();
        product!.Id.Should().Be(createdProduct.Id);
        product.Name.Should().Be(createRequest.Name);
        product.Description.Should().Be(createRequest.Description);
        product.CategoryId.Should().Be(createRequest.CategoryId);
        product.Price.Should().Be(createRequest.Price);
        product.StockQuantity.Should().Be(createRequest.StockQuantity);
        _output.WriteLine("Product retrieval test completed successfully");
    }

    [Fact(DisplayName = "GET /api/products/{id} - Should return 404 for non-existing product")]
    public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
    {
        var nonExistingId = Guid.NewGuid();
        _output.WriteLine($"Testing GET request for non-existing product ID: {nonExistingId}");
        
        // Act
        var response = await _client.GetAsync($"/api/products/{nonExistingId}");
        _output.WriteLine($"Received response with status code: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _output.WriteLine("Successfully verified 404 Not Found response");
    }
}