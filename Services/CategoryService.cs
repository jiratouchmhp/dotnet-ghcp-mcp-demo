using System.Diagnostics;
using Backend.Dtos;
using Backend.Models;
using Backend.Repository;
using Mapster;

namespace Backend.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Services.CategoryService");

    public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryDto?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetCategory", ActivityKind.Internal);
        activity?.SetTag("category.id", id);

        try
        {
            var category = await _repository.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                _logger.LogInformation("Category {CategoryId} not found", id);
                return null;
            }

            var categoryDto = category.Adapt<CategoryDto>();
            activity?.SetTag("category.name", category.Name);
            _logger.LogDebug("Successfully retrieved category {CategoryId}", id);
            
            return categoryDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAllCategories", ActivityKind.Internal);

        try
        {
            var categories = await _repository.GetAllAsync(cancellationToken);
            var categoryDtos = categories.Adapt<IEnumerable<CategoryDto>>();

            activity?.SetTag("categories.count", categories.Count());
            _logger.LogDebug("Retrieved {Count} categories", categories.Count());

            return categoryDtos;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateCategory", ActivityKind.Internal);
        activity?.SetTag("category.name", request.Name);

        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(category, cancellationToken);
            var categoryDto = category.Adapt<CategoryDto>();

            activity?.SetTag("category.id", category.Id);
            _logger.LogInformation("Created new category {CategoryId}", category.Id);

            return categoryDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating category {CategoryName}", request.Name);
            throw;
        }
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("UpdateCategory", ActivityKind.Internal);
        activity?.SetTag("category.id", id);

        try
        {
            var existingCategory = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found");
            }

            existingCategory.Name = request.Name;
            existingCategory.Description = request.Description;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            var updatedCategory = await _repository.UpdateAsync(existingCategory, cancellationToken);
            var categoryDto = updatedCategory.Adapt<CategoryDto>();

            activity?.SetTag("category.name", categoryDto.Name);
            _logger.LogInformation("Updated category {CategoryId}", id);

            return categoryDto;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("DeleteCategory", ActivityKind.Internal);
        activity?.SetTag("category.id", id);

        try
        {
            var result = await _repository.DeleteAsync(id, cancellationToken);
            if (result)
            {
                _logger.LogInformation("Deleted category {CategoryId}", id);
            }
            else
            {
                _logger.LogInformation("Category {CategoryId} not found for deletion", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            throw;
        }
    }
}