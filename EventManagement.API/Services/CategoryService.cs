using EventManagement.API.DTOs.Categories;
using EventManagement.API.DTOs.Common;
using EventManagement.API.Entities;
using EventManagement.API.Repositories.Interfaces;
using EventManagement.API.Services.Interfaces;

namespace EventManagement.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();
        return categories.Select(MapToResponse);
    }

    public async Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request)
    {
        if (await _repo.NameExistsAsync(request.Name))
            return ServiceResult<CategoryResponse>.Conflict($"Category '{request.Name}' already exists.");

        var category = new Category { Name = request.Name };
        await _repo.CreateAsync(category);

        return ServiceResult<CategoryResponse>.Ok(MapToResponse(category));
    }

    public async Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category is null)
            return ServiceResult<CategoryResponse>.NotFound($"Category with id {id} not found.");

        if (await _repo.NameExistsForOtherAsync(request.Name, id))
            return ServiceResult<CategoryResponse>.Conflict($"Category '{request.Name}' already exists.");

        category.Name = request.Name;
        await _repo.UpdateAsync(category);

        return ServiceResult<CategoryResponse>.Ok(MapToResponse(category));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category is null)
            return ServiceResult<bool>.NotFound($"Category with id {id} not found.");

        if (await _repo.HasEventsAsync(id))
            return ServiceResult<bool>.Conflict("Cannot delete category: one or more events reference it.");

        await _repo.DeleteAsync(category);
        return ServiceResult<bool>.Ok(true);
    }

    private static CategoryResponse MapToResponse(Category c) => new() { Id = c.Id, Name = c.Name };
}