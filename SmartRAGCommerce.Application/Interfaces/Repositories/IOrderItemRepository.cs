
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Interfaces.Repositories;

public interface IOrderItemRepository
{
    Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
}
