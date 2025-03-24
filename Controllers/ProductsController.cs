using System.Diagnostics;
using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for managing product-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Controllers.ProductsController");

    public ProductsController(ProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of all products.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GetProducts", ActivityKind.Server);

        try
        {
            var products = await _productService.GetAllProductsAsync(cancellationToken);
            activity?.SetTag("products.count", products.Count());
            return Ok(products);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    /// <summary>Gets a product by ID</summary>
    /// <param name="id">ID of the product</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The product if found</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetProduct", ActivityKind.Server);
        activity?.SetTag("product.id", id);

        try
        {
            var product = await _productService.GetProductAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            activity?.SetTag("product.name", product.Name);
            return Ok(product);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>Creates a product</summary>
    /// <param name="request">Product data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product</returns>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateProduct", ActivityKind.Server);
        activity?.SetTag("product.name", request.Name);

        try
        {
            var product = await _productService.CreateProductAsync(request, cancellationToken);
            
            activity?.SetTag("product.id", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating product {ProductName}", request.Name);
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    /// <summary>Updates a product</summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        Guid id,
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateProduct", ActivityKind.Server);
        activity?.SetTag("product.id", id);
        activity?.SetTag("product.name", request.Name);

        try
        {
            var product = await _productService.UpdateProductAsync(id, request, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    /// <summary>Deletes a product</summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("DeleteProduct", ActivityKind.Server);
        activity?.SetTag("product.id", id);

        try
        {
            var result = await _productService.DeleteProductAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }
}