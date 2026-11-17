
using SmartRAGCommerce.Application.DTOs.OrderItems;
using SmartRAGCommerce.Application.DTOs.Orders;
using SmartRAGCommerce.Application.DTOs.Payments;
using SmartRAGCommerce.Application.Interfaces.Orders;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderResponseDto>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToResponse).ToList();
    }

    public async Task<OrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order == null ? null : MapToResponse(order);
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto)
    {
        if (!await _orderRepository.CustomerExistsAsync(dto.CustomerId))
            throw new InvalidOperationException("Customer not found.");

        if (dto.Items.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicate products are not allowed.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItem>()
        };

        var products = new List<(CreateOrderItemDto Item, Product Product)>();

        // Validate all products and stock before making changes.
        foreach (var item in dto.Items)
        {
            var product = await _orderRepository.GetProductAsync(item.ProductId);

            if (product == null)
                throw new InvalidOperationException(
                    $"Product ID {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product: {product.Name}");

            products.Add((item, product));
        }

        // Create order items and deduct stock.
        foreach (var entry in products)
        {
            entry.Product.StockQuantity -= entry.Item.Quantity;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = entry.Product.Id,
                Quantity = entry.Item.Quantity,
                UnitPrice = entry.Product.Price
            });

            order.TotalAmount += entry.Product.Price * entry.Item.Quantity;
        }

        var createdOrder = await _orderRepository.AddAsync(order);

        var savedOrder = await _orderRepository.GetByIdAsync(createdOrder.Id);

        return MapToResponse(savedOrder!);
    }

    public async Task UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
            throw new KeyNotFoundException("Order not found.");

        if (dto.Status == "Cancelled" &&
            await _orderRepository.HasCompletedPaymentsAsync(id))
        {
            throw new InvalidOperationException(
                "Cannot cancel an order with completed payments.");
        }

        order.Status = dto.Status;

        await _orderRepository.UpdateAsync(order);
    }

    private static OrderResponseDto MapToResponse(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Items = order.OrderItems.Select(item => new OrderItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                SubTotal = item.Quantity * item.UnitPrice
            }).ToList(),
            Payments = order.Payments.Select(payment => new PaymentResponseDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                PaymentDate = payment.PaymentDate
            }).ToList()
        };
    }
}
