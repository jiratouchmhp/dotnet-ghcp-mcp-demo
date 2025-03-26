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
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;
    private static readonly ActivitySource _activitySource = new("CustomersController");

    public CustomersController(CustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        using var activity = _activitySource.StartActivity("GetCustomers");
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers.Adapt<IEnumerable<CustomerDto>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all customers");
            return StatusCode(500, new ApiError("An error occurred while retrieving customers"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        using var activity = _activitySource.StartActivity("GetCustomer");
        activity?.SetTag("customer.id", id);

        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new ApiError($"Customer with ID {id} not found"));
            }
            return Ok(customer.Adapt<CustomerDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, new ApiError("An error occurred while retrieving the customer"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerRequest request)
    {
        using var activity = _activitySource.StartActivity("CreateCustomer");
        activity?.SetTag("customer.email", request.Email);

        try
        {
            var customer = request.Adapt<Customer>();
            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            if (createdCustomer == null)
            {
                return BadRequest(new ApiError($"Customer with email {request.Email} already exists"));
            }
            var customerDto = createdCustomer.Adapt<CustomerDto>();
            return CreatedAtAction(nameof(GetCustomer), new { id = customerDto.Id }, customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with email {Email}", request.Email);
            return StatusCode(500, new ApiError("An error occurred while creating the customer"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, CreateCustomerRequest request)
    {
        using var activity = _activitySource.StartActivity("UpdateCustomer");
        activity?.SetTag("customer.id", id);

        try
        {
            var customer = request.Adapt<Customer>();
            var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customer);
            if (updatedCustomer == null)
            {
                return NotFound(new ApiError($"Customer with ID {id} not found"));
            }
            return Ok(updatedCustomer.Adapt<CustomerDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, new ApiError("An error occurred while updating the customer"));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        using var activity = _activitySource.StartActivity("DeleteCustomer");
        activity?.SetTag("customer.id", id);

        try
        {
            var deleted = await _customerService.DeleteCustomerAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiError($"Customer with ID {id} not found"));
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, new ApiError("An error occurred while deleting the customer"));
        }
    }
}