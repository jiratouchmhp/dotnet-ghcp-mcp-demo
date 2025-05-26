using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    private static readonly ActivitySource _activitySource = new("PaymentsController");

    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments
    /// </summary>
    /// <returns>List of all payments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
    {
        using var activity = _activitySource.StartActivity("GetPayments");
        try
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments.Adapt<IEnumerable<PaymentDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all payments");
            return StatusCode(500, new ApiError("An error occurred while retrieving payments"));
        }
    }

    /// <summary>
    /// Get a specific payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> GetPayment(int id)
    {
        using var activity = _activitySource.StartActivity("GetPayment");
        activity?.SetTag("payment.id", id);

        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            return Ok(payment.Adapt<PaymentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, new ApiError("An error occurred while retrieving the payment"));
        }
    }

    /// <summary>
    /// Get payments by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of payments for the specified user</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByUser(int userId)
    {
        using var activity = _activitySource.StartActivity("GetPaymentsByUser");
        activity?.SetTag("payment.userId", userId);

        try
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments.Adapt<IEnumerable<PaymentDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
            return StatusCode(500, new ApiError("An error occurred while retrieving payments for the user"));
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="request">Payment details</param>
    /// <returns>Newly created payment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentRequest request)
    {
        using var activity = _activitySource.StartActivity("CreatePayment");
        activity?.SetTag("payment.amount", request.Amount);
        activity?.SetTag("payment.userId", request.UserId);

        try
        {
            var payment = request.Adapt<Payment>();
            payment.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;
            var createdPayment = await _paymentService.CreatePaymentAsync(payment);
            var paymentDto = createdPayment.Adapt<PaymentDto>();
            return CreatedAtAction(nameof(GetPayment), new { id = paymentDto.Id }, paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for user {UserId} with amount {Amount}", 
                request.UserId, request.Amount);
            return StatusCode(500, new ApiError("An error occurred while creating the payment"));
        }
    }

    /// <summary>
    /// Update an existing payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="request">Updated payment details</param>
    /// <returns>Updated payment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentDto>> UpdatePayment(int id, CreatePaymentRequest request)
    {
        using var activity = _activitySource.StartActivity("UpdatePayment");
        activity?.SetTag("payment.id", id);

        try
        {
            var payment = request.Adapt<Payment>();
            payment.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;
            var updatedPayment = await _paymentService.UpdatePaymentAsync(id, payment);
            if (updatedPayment == null)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            return Ok(updatedPayment.Adapt<PaymentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId}", id);
            return StatusCode(500, new ApiError("An error occurred while updating the payment"));
        }
    }

    /// <summary>
    /// Delete a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePayment(int id)
    {
        using var activity = _activitySource.StartActivity("DeletePayment");
        activity?.SetTag("payment.id", id);

        try
        {
            var deleted = await _paymentService.DeletePaymentAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiError($"Payment with ID {id} not found"));
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment {PaymentId}", id);
            return StatusCode(500, new ApiError("An error occurred while deleting the payment"));
        }
    }
}