
using Microsoft.EntityFrameworkCore;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;
using SmartRAGCommerce.Infrastructure.Persistence.Context;

namespace SmartRAGCommerce.Infrastructure.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Product?> GetProductAsync(int productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CustomerExistsAsync(int customerId)
    {
        return await _context.Customers
            .AnyAsync(c => c.Id == customerId);
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == productId);
    }

    public async Task<bool> HasCompletedPaymentsAsync(int orderId)
    {
        return await _context.Payments.AnyAsync(p =>
            p.OrderId == orderId &&
            p.PaymentStatus == "Completed");
    }
}
