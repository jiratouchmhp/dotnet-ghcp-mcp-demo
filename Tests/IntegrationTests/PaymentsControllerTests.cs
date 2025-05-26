using System;
using System.Collections.Generic;
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
    public async Task GetPayments_ShouldReturnEmptyList_WhenDatabaseIsEmpty()
    {
        // Act
        var response = await _client.GetAsync("/api/payments");
        var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payments.Should().NotBeNull();
        payments.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePayment_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending",
            PaymentDate: DateTime.UtcNow,
            UserId: 1,
            TransactionId: "TXN-123456",
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
        paymentDto.Status.Should().Be(request.Status);
        paymentDto.UserId.Should().Be(request.UserId);
        paymentDto.TransactionId.Should().Be(request.TransactionId);
        paymentDto.PaymentMethod.Should().Be(request.PaymentMethod);
        paymentDto.Description.Should().Be(request.Description);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreatePayment_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreatePaymentRequest(
            Amount: -100.00m,
            Currency: "INVALID",
            Status: ""
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPayment_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        var createRequest = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending",
            UserId: 1
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();

        // Act
        var response = await _client.GetAsync($"/api/payments/{createdPayment!.Id}");
        var paymentDto = await response.Content.ReadFromJsonAsync<PaymentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentDto.Should().NotBeNull();
        paymentDto!.Id.Should().Be(createdPayment.Id);
        paymentDto.Amount.Should().Be(createRequest.Amount);
        paymentDto.Currency.Should().Be(createRequest.Currency);
        paymentDto.Status.Should().Be(createRequest.Status);
    }

    [Fact]
    public async Task GetPayment_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/payments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaymentsByUser_WithValidUserId_ShouldReturnUserPayments()
    {
        // Arrange
        var userId = 1;
        
        // Create two payments for user 1
        var request1 = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Completed",
            UserId: userId
        );
        await _client.PostAsJsonAsync("/api/payments", request1);
        
        var request2 = new CreatePaymentRequest(
            Amount: 75.50m,
            Currency: "EUR",
            Status: "Pending",
            UserId: userId
        );
        await _client.PostAsJsonAsync("/api/payments", request2);
        
        // Create one payment for user 2
        var request3 = new CreatePaymentRequest(
            Amount: 50.25m,
            Currency: "USD",
            Status: "Completed",
            UserId: 2
        );
        await _client.PostAsJsonAsync("/api/payments", request3);

        // Act
        var response = await _client.GetAsync($"/api/payments/user/{userId}");
        var payments = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payments.Should().NotBeNull();
        payments.Should().HaveCount(2);
        payments!.ForEach(p => p.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task UpdatePayment_WithValidData_ShouldReturnUpdatedPayment()
    {
        // Arrange
        var createRequest = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending",
            Description: "Original description"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();

        var updateRequest = new CreatePaymentRequest(
            Amount: 150.00m,
            Currency: "USD",
            Status: "Completed",
            Description: "Updated description"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/payments/{createdPayment!.Id}", updateRequest);
        var updatedPayment = await response.Content.ReadFromJsonAsync<PaymentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Id.Should().Be(createdPayment.Id);
        updatedPayment.Amount.Should().Be(updateRequest.Amount);
        updatedPayment.Status.Should().Be(updateRequest.Status);
        updatedPayment.Description.Should().Be(updateRequest.Description);
    }

    [Fact]
    public async Task UpdatePayment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var updateRequest = new CreatePaymentRequest(
            Amount: 150.00m,
            Currency: "USD",
            Status: "Completed"
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/payments/999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePayment_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreatePaymentRequest(
            Amount: 100.00m,
            Currency: "USD",
            Status: "Pending"
        );
        var createResponse = await _client.PostAsJsonAsync("/api/payments", createRequest);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PaymentDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/payments/{createdPayment!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify payment is deleted
        var getResponse = await _client.GetAsync($"/api/payments/{createdPayment.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePayment_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/payments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}