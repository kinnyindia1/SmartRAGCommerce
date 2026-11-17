using SmartRAGCommerce.Application.DTOs.Orders;

namespace SmartRAGCommerce.Application.Interfaces.Orders;

public interface IOrderService
{
    Task<List<OrderResponseDto>> GetAllAsync();

    Task<OrderResponseDto?> GetByIdAsync(int id);

    Task<OrderResponseDto> CreateAsync(CreateOrderDto dto);

    Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
}