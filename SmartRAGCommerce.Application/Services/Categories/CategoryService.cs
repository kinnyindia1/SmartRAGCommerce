
using SmartRAGCommerce.Application.DTOs.Categories;
using SmartRAGCommerce.Application.Interfaces.Categories;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        return category == null ? null : MapToResponse(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.NameExistsAsync(dto.Name))
            throw new InvalidOperationException("Category name already exists.");

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        var created = await _categoryRepository.AddAsync(category);

        return MapToResponse(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return false;

        if (await _categoryRepository.NameExistsAsync(dto.Name, id))
            throw new InvalidOperationException("Category name already exists.");

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _categoryRepository.UpdateAsync(category);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return false;

        if (await _categoryRepository.HasProductsAsync(id))
            throw new InvalidOperationException(
                "Cannot delete this category because it contains products.");

        await _categoryRepository.DeleteAsync(category);

        return true;
    }

    private static CategoryResponseDto MapToResponse(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = category.Products.Count
        };
    }
}
