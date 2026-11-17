
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<Product> AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);

    Task<bool> CategoryExistsAsync(int categoryId);
}
