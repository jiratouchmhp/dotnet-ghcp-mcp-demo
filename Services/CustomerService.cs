using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Repository;
using Microsoft.Extensions.Logging;
using Mapster;

namespace Backend.Services;

public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;
    private static readonly ActivitySource _activitySource = new("CustomerService");

    public CustomerService(ICustomerRepository repository, ILogger<CustomerService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        using var activity = _activitySource.StartActivity("GetAllCustomers");
        _logger.LogInformation("Retrieving all customers");
        return await _repository.GetAllAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        using var activity = _activitySource.StartActivity("GetCustomerById");
        activity?.SetTag("customer.id", id);
        
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
        var customer = await _repository.GetByIdAsync(id);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
        }
        
        return customer;
    }

    public async Task<Customer?> CreateCustomerAsync(Customer customer)
    {
        using var activity = _activitySource.StartActivity("CreateCustomer");
        activity?.SetTag("customer.email", customer.Email);
        
        var existingCustomer = await _repository.GetByEmailAsync(customer.Email);
        if (existingCustomer != null)
        {
            _logger.LogWarning("Customer with email {Email} already exists", customer.Email);
            return null;
        }

        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Creating new customer with email: {Email}", customer.Email);
        return await _repository.CreateAsync(customer);
    }

    public async Task<Customer?> UpdateCustomerAsync(int id, Customer customer)
    {
        using var activity = _activitySource.StartActivity("UpdateCustomer");
        activity?.SetTag("customer.id", id);
        
        var existingCustomer = await _repository.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
            return null;
        }

        // Check if email is being changed and if it's already in use
        if (customer.Email != existingCustomer.Email)
        {
            var customerWithEmail = await _repository.GetByEmailAsync(customer.Email);
            if (customerWithEmail != null)
            {
                _logger.LogWarning("Email {Email} is already in use", customer.Email);
                return null;
            }
        }

        customer.Id = id;
        customer.CreatedAt = existingCustomer.CreatedAt;
        customer.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
        return await _repository.UpdateAsync(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        using var activity = _activitySource.StartActivity("DeleteCustomer");
        activity?.SetTag("customer.id", id);
        
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
        return await _repository.DeleteAsync(id);
    }
}