using System.Diagnostics;
using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for managing category-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Controllers.CategoriesController");

    public CategoriesController(CategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all categories
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A list of all categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("GetCategories", ActivityKind.Server);
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            activity?.SetTag("categories.count", categories.Count());
            return Ok(categories);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all categories");
            return StatusCode(500, new ApiError("An error occurred while retrieving categories"));
        }
    }

    /// <summary>Gets a category by ID</summary>
    /// <param name="id">ID of the category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category if found</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetCategory", ActivityKind.Server);
        activity?.SetTag("category.id", id);
        try
        {
            var category = await _categoryService.GetCategoryAsync(id, cancellationToken);
            if (category == null)
            {
                return NotFound(new ApiError($"Category with ID {id} not found"));
            }
            activity?.SetTag("category.name", category.Name);
            return Ok(category);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, new ApiError("An error occurred while retrieving the category"));
        }
    }

    /// <summary>Creates a category</summary>
    /// <param name="request">Category data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateCategory", ActivityKind.Server);
        activity?.SetTag("category.name", request.Name);
        try
        {
            var category = await _categoryService.CreateCategoryAsync(request, cancellationToken);
            activity?.SetTag("category.id", category.Id);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating category {CategoryName}", request.Name);
            return StatusCode(500, new ApiError("An error occurred while creating the category"));
        }
    }

    /// <summary>Updates a category</summary>
    /// <param name="id">Category ID</param>
    /// <param name="request">Updated data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        Guid id,
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateCategory", ActivityKind.Server);
        activity?.SetTag("category.id", id);
        activity?.SetTag("category.name", request.Name);
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
            return Ok(category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message));
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, new ApiError("An error occurred while updating the category"));
        }
    }

    /// <summary>Deletes a category</summary>
    /// <param name="id">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("DeleteCategory", ActivityKind.Server);
        activity?.SetTag("category.id", id);
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new ApiError($"Category with ID {id} not found"));
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, new ApiError("An error occurred while deleting the category"));
        }
    }
}