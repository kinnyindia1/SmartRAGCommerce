
using SmartRAGCommerce.Application.DTOs.Categories;

namespace SmartRAGCommerce.Application.Interfaces.Categories;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllAsync();

    Task<CategoryResponseDto?> GetByIdAsync(int id);

    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);

    Task<bool> UpdateAsync(int id, UpdateCategoryDto dto);

    Task<bool> DeleteAsync(int id);
}
