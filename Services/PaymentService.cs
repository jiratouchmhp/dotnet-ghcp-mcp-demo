using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentService> _logger;
    private static readonly ActivitySource _activitySource = new("Backend.Services.PaymentService");

    public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetAllPayments", ActivityKind.Internal);
        
        try
        {
            _logger.LogInformation("Retrieving all payments");
            var payments = await _repository.GetAllAsync(cancellationToken);
            return payments.Adapt<IEnumerable<PaymentDto>>();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all payments");
            throw;
        }
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPaymentById", ActivityKind.Internal);
        activity?.SetTag("payment.id", id);
        
        try
        {
            _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
            var payment = await _repository.GetByIdAsync(id, cancellationToken);
            
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
                return null;
            }
            
            return payment.Adapt<PaymentDto>();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPaymentsByUserId", ActivityKind.Internal);
        activity?.SetTag("payment.userId", userId);
        
        try
        {
            _logger.LogInformation("Retrieving payments for user ID: {UserId}", userId);
            var payments = await _repository.GetByUserIdAsync(userId, cancellationToken);
            return payments.Adapt<IEnumerable<PaymentDto>>();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving payments for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CreatePayment", ActivityKind.Internal);
        activity?.SetTag("payment.amount", request.Amount);
        activity?.SetTag("payment.currency", request.Currency);
        
        try
        {
            var payment = new Payment
            {
                Amount = request.Amount,
                Currency = request.Currency,
                Status = request.Status,
                PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
                UserId = request.UserId,
                TransactionId = request.TransactionId,
                PaymentMethod = request.PaymentMethod,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Creating new payment with amount: {Amount} {Currency}", payment.Amount, payment.Currency);
            var createdPayment = await _repository.CreateAsync(payment, cancellationToken);
            
            activity?.SetTag("payment.id", createdPayment.Id);
            return createdPayment.Adapt<PaymentDto>();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating payment");
            throw;
        }
    }

    public async Task<PaymentDto?> UpdatePaymentAsync(int id, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("UpdatePayment", ActivityKind.Internal);
        activity?.SetTag("payment.id", id);
        
        try
        {
            var existingPayment = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingPayment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
                return null;
            }

            existingPayment.Amount = request.Amount;
            existingPayment.Currency = request.Currency;
            existingPayment.Status = request.Status;
            existingPayment.PaymentDate = request.PaymentDate ?? existingPayment.PaymentDate;
            existingPayment.UserId = request.UserId;
            existingPayment.TransactionId = request.TransactionId;
            existingPayment.PaymentMethod = request.PaymentMethod;
            existingPayment.Description = request.Description;
            existingPayment.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updating payment with ID: {PaymentId}", id);
            var updatedPayment = await _repository.UpdateAsync(existingPayment, cancellationToken);
            
            return updatedPayment?.Adapt<PaymentDto>();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            throw;
        }
    }

    public async Task<bool> DeletePaymentAsync(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("DeletePayment", ActivityKind.Internal);
        activity?.SetTag("payment.id", id);
        
        try
        {
            _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);
            return await _repository.DeleteAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            throw;
        }
    }
}