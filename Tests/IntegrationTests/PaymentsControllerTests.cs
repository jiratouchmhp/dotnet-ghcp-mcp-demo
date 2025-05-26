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

public class PaymentsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PaymentsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _factory.ResetDatabase(); // Reset database before each test
    }

    [Fact]
    public async Task CreatePayment_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.50m,
            Currency: "USD",
            Status: "Pending",
            UserId: 1,
            PaymentMethod: "Credit Card",
            Description: "Test payment"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", request);
        var paymentDto = await response.Content.ReadFromJsonAsync<PaymentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        paymentDto.Should().NotBeNull();
        paymentDto!.Amount.Should().Be(request.Amount);
        paymentDto.Currency.Should().Be(request.Currency);
        paymentDto.UserId.Should().Be(request.UserId);
        paymentDto.PaymentMethod.Should().Be(request.PaymentMethod);
        paymentDto.Description.Should().Be(request.Description);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPayment_WithExistingPayment_ShouldReturnPayment()
    {
        // Arrange
        var createRequest = new CreatePaymentRequest(
            Amount: 200.75m,
            Currency: "USD",
            UserId: 2,
            Status: "Completed"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();
        var paymentId = createdPayment!.Id;

        // Act
        var response = await _client.GetAsync($"/api/payments/{paymentId}");
        var paymentDto = await response.Content.ReadFromJsonAsync<PaymentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentDto.Should().NotBeNull();
        paymentDto!.Id.Should().Be(paymentId);
        paymentDto.Amount.Should().Be(createRequest.Amount);
        paymentDto.Currency.Should().Be(createRequest.Currency);
        paymentDto.UserId.Should().Be(createRequest.UserId);
        paymentDto.Status.Should().Be(createRequest.Status);
    }

    [Fact]
    public async Task GetPayment_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/payments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePayment_WithValidData_ShouldReturnUpdatedPayment()
    {
        // Arrange - Create a payment
        var createRequest = new CreatePaymentRequest(
            Amount: 300,
            Currency: "USD",
            Status: "Pending",
            UserId: 3
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();
        var paymentId = createdPayment!.Id;

        // Arrange - Update data
        var updateRequest = new CreatePaymentRequest(
            Amount: 350,
            Currency: "USD",
            Status: "Completed",
            UserId: 3,
            Description: "Updated payment"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/payments/{paymentId}", updateRequest);
        var updatedPayment = await response.Content.ReadFromJsonAsync<PaymentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Id.Should().Be(paymentId);
        updatedPayment.Amount.Should().Be(updateRequest.Amount);
        updatedPayment.Status.Should().Be(updateRequest.Status);
        updatedPayment.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task UpdatePayment_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new CreatePaymentRequest(
            Amount: 350,
            Currency: "USD",
            UserId: 3
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/payments/999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePayment_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange - Create a payment
        var createRequest = new CreatePaymentRequest(
            Amount: 400,
            Currency: "USD",
            UserId: 4
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();
        var paymentId = createdPayment!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/payments/{paymentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await _client.GetAsync($"/api/payments/{paymentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePayment_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange & Act
        var response = await _client.DeleteAsync("/api/payments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaymentsByUser_ShouldReturnUserPayments()
    {
        // Arrange - Create payments for a specific user
        var userId = 5;
        var request1 = new CreatePaymentRequest(
            Amount: 100,
            Currency: "USD",
            UserId: userId
        );
        var request2 = new CreatePaymentRequest(
            Amount: 200,
            Currency: "USD",
            UserId: userId
        );
        
        // Create payments
        await _client.PostAsJsonAsync("/api/payments", request1);
        await _client.PostAsJsonAsync("/api/payments", request2);
        
        // Create a payment for another user to verify filtering
        var otherRequest = new CreatePaymentRequest(
            Amount: 300,
            Currency: "USD",
            UserId: userId + 1
        );
        await _client.PostAsJsonAsync("/api/payments", otherRequest);

        // Act
        var response = await _client.GetAsync($"/api/payments/user/{userId}");
        var payments = await response.Content.ReadFromJsonAsync<PaymentDto[]>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payments.Should().NotBeNull();
        payments!.Length.Should().Be(2);
        payments.Should().AllSatisfy(p => p.UserId.Should().Be(userId));
    }
}