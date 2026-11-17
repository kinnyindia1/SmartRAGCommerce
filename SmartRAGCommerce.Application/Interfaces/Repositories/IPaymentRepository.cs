
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<List<Payment>> GetByOrderIdAsync(int orderId);
    Task<Payment?> GetByIdAsync(int paymentId);
    Task<Payment> AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
