using System.Diagnostics;
using Backend.DbContext;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;
    private static readonly ActivitySource ActivitySource = new("Backend.Repository.CategoryRepository");

    public CategoryRepository(AppDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetById", ActivityKind.Internal);
        activity?.SetTag("category.id", id);

        try
        {
            var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
            
            if (category == null)
            {
                _logger.LogInformation("Category with ID {CategoryId} not found", id);
                return null;
            }

            _logger.LogDebug("Successfully retrieved category {CategoryId}", id);
            return category;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAll", ActivityKind.Internal);

        try
        {
            var categories = await _context.Categories.ToListAsync(cancellationToken);
            activity?.SetTag("categories.count", categories.Count);
            _logger.LogDebug("Retrieved {Count} categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Create", ActivityKind.Internal);
        activity?.SetTag("category.name", category.Name);

        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
            
            activity?.SetTag("category.id", category.Id);
            _logger.LogInformation("Created new category with ID {CategoryId}", category.Id);
            
            return category;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error creating category {CategoryName}", category.Name);
            throw;
        }
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Update", ActivityKind.Internal);
        activity?.SetTag("category.id", category.Id);
        try
        {
            var existingCategory = await _context.Categories.FindAsync(new object[] { category.Id }, cancellationToken);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {category.Id} not found");
            }

            // Detach existing entity to avoid tracking conflicts
            _context.Entry(existingCategory).State = EntityState.Detached;
            
            // Attach and mark as modified
            _context.Categories.Attach(category);
            _context.Entry(category).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated category with ID {CategoryId}", category.Id);
            return category;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error updating category {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("Delete", ActivityKind.Internal);
        activity?.SetTag("category.id", id);

        try
        {
            var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
            if (category == null)
            {
                _logger.LogInformation("Category with ID {CategoryId} not found for deletion", id);
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Deleted category with ID {CategoryId}", id);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", ex.Message);
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            throw;
        }
    }
}