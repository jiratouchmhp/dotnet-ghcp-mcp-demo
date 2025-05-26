using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers;

/// <summary>
/// Controller for managing payment-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    private static readonly ActivitySource _activitySource = new("Backend.Controllers.PaymentsController");

    /// <summary>
    /// Constructor for the PaymentsController
    /// </summary>
    /// <param name="paymentService">Payment service</param>
    /// <param name="logger">Logger</param>
    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all payments
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all payments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPayments");
        
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync(cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments");
            return StatusCode(500, new ApiError("An error occurred while retrieving payments"));
        }
    }

    /// <summary>
    /// Gets a payment by ID
    /// </summary>
    /// <param name="id">ID of the payment</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The payment if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> GetPayment(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPayment");
        activity?.SetTag("payment.id", id);
        
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
            
            if (payment == null)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            return StatusCode(500, new ApiError($"An error occurred while retrieving payment {id}"));
        }
    }

    /// <summary>
    /// Gets payments by user ID
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>The list of payments for the user</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByUser(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("GetPaymentsByUser");
        activity?.SetTag("payment.userId", userId);
        
        try
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId, cancellationToken);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for user {UserId}", userId);
            return StatusCode(500, new ApiError($"An error occurred while retrieving payments for user {userId}"));
        }
    }

    /// <summary>
    /// Creates a payment
    /// </summary>
    /// <param name="request">Payment data</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Created payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CreatePayment");
        activity?.SetTag("payment.amount", request.Amount);
        activity?.SetTag("payment.currency", request.Currency);
        
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new ApiError("An error occurred while creating the payment"));
        }
    }

    /// <summary>
    /// Updates a payment
    /// </summary>
    /// <param name="id">ID of the payment to update</param>
    /// <param name="request">Payment data</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Updated payment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> UpdatePayment(int id, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("UpdatePayment");
        activity?.SetTag("payment.id", id);
        
        try
        {
            var payment = await _paymentService.UpdatePaymentAsync(id, request, cancellationToken);
            
            if (payment == null)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return StatusCode(500, new ApiError($"An error occurred while updating payment {id}"));
        }
    }

    /// <summary>
    /// Deletes a payment
    /// </summary>
    /// <param name="id">ID of the payment to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePayment(int id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("DeletePayment");
        activity?.SetTag("payment.id", id);
        
        try
        {
            var result = await _paymentService.DeletePaymentAsync(id, cancellationToken);
            
            if (!result)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return StatusCode(500, new ApiError($"An error occurred while deleting payment {id}"));
        }
    }
}