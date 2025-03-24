using System.Net;
using System.Text;
using System.Text.Json;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Hosting;

namespace Backend.Tests.IntegrationTests;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
        });
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact(DisplayName = "POST /api/products - Should create new product with valid data")]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var request = new CreateProductRequest(
            "Integration Test Product",
            "Test Description",
            29.99m,
            100
        );

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var responseContent = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, _jsonOptions);

        product.Should().NotBeNull();
        product!.Name.Should().Be(request.Name);
        product.Description.Should().Be(request.Description);
        product.Price.Should().Be(request.Price);
        product.StockQuantity.Should().Be(request.StockQuantity);
    }

    [Fact(DisplayName = "GET /api/products/{id} - Should return existing product")]
    public async Task GetProduct_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        // First create a product
        var createRequest = new CreateProductRequest(
            "Test Get Product",
            "Test Description",
            39.99m,
            50
        );

        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            Encoding.UTF8,
            "application/json"
        );

        var createResponse = await _client.PostAsync("/api/products", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<ProductDto>(createResponseContent, _jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/products/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(responseContent, _jsonOptions);

        product.Should().NotBeNull();
        product!.Id.Should().Be(createdProduct.Id);
        product.Name.Should().Be(createRequest.Name);
        product.Description.Should().Be(createRequest.Description);
        product.Price.Should().Be(createRequest.Price);
        product.StockQuantity.Should().Be(createRequest.StockQuantity);
    }

    [Fact(DisplayName = "GET /api/products/{id} - Should return 404 for non-existing product")]
    public async Task GetProduct_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}