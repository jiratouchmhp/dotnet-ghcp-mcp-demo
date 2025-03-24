using System.Diagnostics;
using Backend.DbContext;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductRepository> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Repository.ProductRepository");

    public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetById", ActivityKind.Internal);
        activity?.SetTag("product.id", id);

        try
        {
            var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            
            if (product == null)
            {
                _logger.LogInformation("Product with ID {ProductId} not found", id);
                return null;
            }

            _logger.LogDebug("Successfully retrieved product {ProductId}", id);
            return product;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAll", ActivityKind.Internal);

        try
        {
            var products = await _context.Products.ToListAsync(cancellationToken);
            activity?.SetTag("products.count", products.Count);
            _logger.LogDebug("Retrieved {Count} products", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Create", ActivityKind.Internal);
        activity?.SetTag("product.name", product.Name);

        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            
            activity?.SetTag("product.id", product.Id);
            _logger.LogInformation("Created new product with ID {ProductId}", product.Id);
            
            return product;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating product {ProductName}", product.Name);
            throw;
        }
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Update", ActivityKind.Internal);
        activity?.SetTag("product.id", product.Id);
        try
        {
            var existingProduct = await _context.Products.FindAsync(new object[] { product.Id }, cancellationToken);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found");
            }

            // Detach existing entity to avoid tracking conflicts
            _context.Entry(existingProduct).State = EntityState.Detached;
            
            // Attach and mark as modified
            _context.Products.Attach(product);
            _context.Entry(product).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated product with ID {ProductId}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating product {ProductId}", product.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Delete", ActivityKind.Internal);
        activity?.SetTag("product.id", id);

        try
        {
            var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
            if (product == null)
            {
                _logger.LogInformation("Product with ID {ProductId} not found for deletion", id);
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Deleted product with ID {ProductId}", id);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }
}