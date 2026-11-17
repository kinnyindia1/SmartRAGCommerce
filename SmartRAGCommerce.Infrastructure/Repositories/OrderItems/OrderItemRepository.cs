
using Microsoft.EntityFrameworkCore;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;
using SmartRAGCommerce.Infrastructure.Persistence.Context;

namespace SmartRAGCommerce.Infrastructure.Repositories.OrderItems;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly ApplicationDbContext _context;

    public OrderItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderItems
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.OrderId == orderId)
            .ToListAsync();
    }
}
