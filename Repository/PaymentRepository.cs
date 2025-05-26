using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.DbContext;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;
    private static readonly ActivitySource _activitySource = new("PaymentRepository");

    public PaymentRepository(AppDbContext context, ILogger<PaymentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetAllPayments");
        _logger.LogInformation("Retrieving all payments");
        return await _context.Payments.ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPaymentById");
        activity?.SetTag("payment.id", id);
        _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);
        return await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPaymentsByUserId");
        activity?.SetTag("payment.userId", userId);
        _logger.LogInformation("Retrieving payments for user ID: {UserId}", userId);
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CreatePayment");
        activity?.SetTag("payment.amount", payment.Amount);
        activity?.SetTag("payment.currency", payment.Currency);
        
        _logger.LogInformation("Creating new payment with amount: {Amount} {Currency}", payment.Amount, payment.Currency);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment?> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("UpdatePayment");
        activity?.SetTag("payment.id", payment.Id);
        
        _logger.LogInformation("Updating payment with ID: {PaymentId}", payment.Id);
        var existingPayment = await _context.Payments.FindAsync(new object[] { payment.Id }, cancellationToken);
        
        if (existingPayment == null)
        {
            _logger.LogWarning("Payment not found with ID: {PaymentId}", payment.Id);
            return null;
        }

        _context.Entry(existingPayment).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return existingPayment;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("DeletePayment");
        activity?.SetTag("payment.id", id);
        
        _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);
        var payment = await _context.Payments.FindAsync(new object[] { id }, cancellationToken);
        
        if (payment == null)
        {
            _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
            return false;
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}