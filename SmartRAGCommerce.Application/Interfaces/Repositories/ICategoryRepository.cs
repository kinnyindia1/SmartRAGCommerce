
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<Category> AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);

    Task<bool> NameExistsAsync(string name, int? excludeCategoryId = null);

    Task<bool> HasProductsAsync(int categoryId);
}
