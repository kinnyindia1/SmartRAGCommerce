
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> CustomerExistsAsync(int customerId);
    Task<bool> ProductExistsAsync(int productId);
    Task<bool> HasCompletedPaymentsAsync(int orderId);
    Task<Product?> GetProductAsync(int productId);
}
