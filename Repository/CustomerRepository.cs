using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Backend.DbContext;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;
    private static readonly ActivitySource _activitySource = new("CustomerRepository");

    public CustomerRepository(AppDbContext context, ILogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        using var activity = _activitySource.StartActivity("GetAllCustomers");
        _logger.LogInformation("Retrieving all customers");
        return await _context.Customers.ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        using var activity = _activitySource.StartActivity("GetCustomerById");
        activity?.SetTag("customer.id", id);
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
        return await _context.Customers.FindAsync(id);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        using var activity = _activitySource.StartActivity("GetCustomerByEmail");
        activity?.SetTag("customer.email", email);
        _logger.LogInformation("Retrieving customer with email: {Email}", email);
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        using var activity = _activitySource.StartActivity("CreateCustomer");
        activity?.SetTag("customer.email", customer.Email);
        
        _logger.LogInformation("Creating new customer with email: {Email}", customer.Email);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> UpdateAsync(Customer customer)
    {
        using var activity = _activitySource.StartActivity("UpdateCustomer");
        activity?.SetTag("customer.id", customer.Id);
        
        _logger.LogInformation("Updating customer with ID: {CustomerId}", customer.Id);
        var existingCustomer = await _context.Customers.FindAsync(customer.Id);
        
        if (existingCustomer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", customer.Id);
            return null;
        }

        _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
        await _context.SaveChangesAsync();
        return existingCustomer;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var activity = _activitySource.StartActivity("DeleteCustomer");
        activity?.SetTag("customer.id", id);
        
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);
        var customer = await _context.Customers.FindAsync(id);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
            return false;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}