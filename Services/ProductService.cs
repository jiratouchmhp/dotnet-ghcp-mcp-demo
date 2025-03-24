using System.Diagnostics;
using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Mapster;

namespace Backend.Services;

public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Services.ProductService");

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetProduct", ActivityKind.Internal);
        activity?.SetTag("product.id", id);

        try
        {
            var product = await _repository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                _logger.LogInformation("Product {ProductId} not found", id);
                return null;
            }

            var productDto = product.Adapt<ProductDto>();
            activity?.SetTag("product.name", product.Name);
            _logger.LogDebug("Successfully retrieved product {ProductId}", id);
            
            return productDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAllProducts", ActivityKind.Internal);

        try
        {
            var products = await _repository.GetAllAsync(cancellationToken);
            var productDtos = products.Adapt<IEnumerable<ProductDto>>();

            activity?.SetTag("products.count", products.Count());
            _logger.LogDebug("Retrieved {Count} products", products.Count());

            return productDtos;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateProduct", ActivityKind.Internal);
        activity?.SetTag("product.name", request.Name);
        activity?.SetTag("product.category_id", request.CategoryId);

        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(product, cancellationToken);
            var productDto = product.Adapt<ProductDto>();

            activity?.SetTag("product.id", product.Id);
            _logger.LogInformation("Created new product {ProductId} in category {CategoryId}", product.Id, product.CategoryId);

            return productDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating product {ProductName}", request.Name);
            throw;
        }
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateProduct", ActivityKind.Internal);
        activity?.SetTag("product.id", id);
        activity?.SetTag("product.name", request.Name);

        try
        {
            var existingProduct = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            existingProduct.Name = request.Name;
            existingProduct.Description = request.Description;
            existingProduct.Price = request.Price;
            existingProduct.StockQuantity = request.StockQuantity;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingProduct, cancellationToken);
            var productDto = existingProduct.Adapt<ProductDto>();

            _logger.LogInformation("Updated product {ProductId}", id);
            return productDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("DeleteProduct", ActivityKind.Internal);
        activity?.SetTag("product.id", id);

        try
        {
            var result = await _repository.DeleteAsync(id, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Deleted product {ProductId}", id);
            }
            else
            {
                _logger.LogInformation("Product {ProductId} not found for deletion", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }
}