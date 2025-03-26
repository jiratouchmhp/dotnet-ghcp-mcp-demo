using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class CustomersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _factory.ResetDatabase(); // Reset database before each test
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            PhoneNumber: "1234567890"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);
        var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        customerDto.Should().NotBeNull();
        customerDto!.FirstName.Should().Be(request.FirstName);
        customerDto.LastName.Should().Be(request.LastName);
        customerDto.Email.Should().Be(request.Email);
        customerDto.PhoneNumber.Should().Be(request.PhoneNumber);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest(
            FirstName: "Jane",
            LastName: "Smith",
            Email: "jane.smith@example.com",
            PhoneNumber: "0987654321"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        // Act
        var response = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}");
        var customerDto = await response.Content.ReadFromJsonAsync<CustomerDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        customerDto.Should().NotBeNull();
        customerDto!.Id.Should().Be(createdCustomer.Id);
        customerDto.FirstName.Should().Be(createRequest.FirstName);
        customerDto.LastName.Should().Be(createRequest.LastName);
        customerDto.Email.Should().Be(createRequest.Email);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest(
            FirstName: "Original",
            LastName: "Name",
            Email: "original@example.com",
            PhoneNumber: "1234567890"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var updateRequest = new CreateCustomerRequest(
            FirstName: "Updated",
            LastName: "Name",
            Email: "updated@example.com",
            PhoneNumber: "0987654321"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{createdCustomer!.Id}", updateRequest);
        var updatedCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Id.Should().Be(createdCustomer.Id);
        updatedCustomer.FirstName.Should().Be(updateRequest.FirstName);
        updatedCustomer.LastName.Should().Be(updateRequest.LastName);
        updatedCustomer.Email.Should().Be(updateRequest.Email);
        updatedCustomer.PhoneNumber.Should().Be(updateRequest.PhoneNumber);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest(
            FirstName: "ToDelete",
            LastName: "Customer",
            Email: "todelete@example.com",
            PhoneNumber: "1234567890"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/customers", createRequest);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify customer is deleted
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}