using EventManagement.API.DTOs.Categories;
using EventManagement.API.DTOs.Common;

namespace EventManagement.API.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<ServiceResult<CategoryResponse>> CreateAsync(CreateCategoryRequest request);
    Task<ServiceResult<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}