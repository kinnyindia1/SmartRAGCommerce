using SmartRAGCommerce.Application.DTOs.Payments;

namespace SmartRAGCommerce.Application.Interfaces.Payments;

public interface IPaymentService
{
    Task<List<PaymentResponseDto>> GetByOrderIdAsync(int orderId);

    Task<PaymentResponseDto> CreateAsync(int orderId, CreatePaymentDto dto);

    Task UpdateStatusAsync(int paymentId, UpdatePaymentStatusDto dto);
}