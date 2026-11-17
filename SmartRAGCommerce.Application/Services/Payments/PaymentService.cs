using SmartRAGCommerce.Application.DTOs.Payments;
using SmartRAGCommerce.Application.Interfaces.Payments;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;

    public PaymentService( IPaymentRepository paymentRepository, IOrderRepository orderRepository)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<List<PaymentResponseDto>> GetByOrderIdAsync(int orderId)
    {
        var payments = await _paymentRepository.GetByOrderIdAsync(orderId);

        return payments.Select(MapToResponse).ToList();
    }

    public async Task<PaymentResponseDto> CreateAsync(int orderId, CreatePaymentDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        if (order.Status == "Cancelled")
            throw new InvalidOperationException(
                "Cannot accept payment for a cancelled order.");

        var completedAmount = order.Payments
            .Where(p => p.PaymentStatus == "Completed")
            .Sum(p => p.Amount);

        if (completedAmount + dto.Amount > order.TotalAmount)
            throw new InvalidOperationException(
                "Payment amount exceeds the outstanding order amount.");

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Pending",
            PaymentDate = DateTime.UtcNow
        };

        var createdPayment = await _paymentRepository.AddAsync(payment);

        return MapToResponse(createdPayment);
    }

    public async Task UpdateStatusAsync(int paymentId, UpdatePaymentStatusDto dto)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);

        if (payment == null)
            throw new KeyNotFoundException("Payment not found.");

        if (payment.PaymentStatus == "Completed")
            throw new InvalidOperationException(
                "Completed payments cannot be modified.");

        if (dto.PaymentStatus == "Completed")
        {
            var order = await _orderRepository.GetByIdAsync(payment.OrderId);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var otherCompletedPayments = order.Payments
                .Where(p => p.Id != payment.Id &&
                            p.PaymentStatus == "Completed")
                .Sum(p => p.Amount);

            if (otherCompletedPayments + payment.Amount > order.TotalAmount)
                throw new InvalidOperationException(
                    "Completed payments cannot exceed the order total.");
        }

        payment.PaymentStatus = dto.PaymentStatus;

        await _paymentRepository.UpdateAsync(payment);
    }

    private static PaymentResponseDto MapToResponse(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            PaymentDate = payment.PaymentDate
        };
    }
}