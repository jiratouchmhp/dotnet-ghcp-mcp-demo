using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentService> _logger;
    private static readonly ActivitySource _activitySource = new("PaymentService");

    public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
    {
        using var activity = _activitySource.StartActivity("GetAllPayments");
        _logger.LogInformation("Retrieving all payments");
        return await _repository.GetAllAsync();
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        using var activity = _activitySource.StartActivity("GetPaymentById");
        activity?.SetTag("payment.id", id);
        
        _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
        var payment = await _repository.GetByIdAsync(id);
        
        if (payment == null)
        {
            _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
        }
        
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
    {
        using var activity = _activitySource.StartActivity("GetPaymentsByUserId");
        activity?.SetTag("payment.userId", userId);
        
        _logger.LogInformation("Retrieving payments for user with ID: {UserId}", userId);
        return await _repository.GetByUserIdAsync(userId);
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        using var activity = _activitySource.StartActivity("CreatePayment");
        activity?.SetTag("payment.amount", payment.Amount);
        activity?.SetTag("payment.userId", payment.UserId);
        
        // Set default values if not provided
        payment.CreatedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        
        if (payment.PaymentDate == default)
        {
            payment.PaymentDate = DateTime.UtcNow;
        }
        
        _logger.LogInformation("Creating new payment for user: {UserId} with amount: {Amount} {Currency}", 
            payment.UserId, payment.Amount, payment.Currency);
        return await _repository.CreateAsync(payment);
    }

    public async Task<Payment?> UpdatePaymentAsync(int id, Payment payment)
    {
        using var activity = _activitySource.StartActivity("UpdatePayment");
        activity?.SetTag("payment.id", id);
        
        var existingPayment = await _repository.GetByIdAsync(id);
        if (existingPayment == null)
        {
            _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
            return null;
        }

        payment.Id = id;
        payment.CreatedAt = existingPayment.CreatedAt;
        payment.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updating payment with ID: {PaymentId}", id);
        return await _repository.UpdateAsync(payment);
    }

    public async Task<bool> DeletePaymentAsync(int id)
    {
        using var activity = _activitySource.StartActivity("DeletePayment");
        activity?.SetTag("payment.id", id);
        
        _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);
        return await _repository.DeleteAsync(id);
    }
}