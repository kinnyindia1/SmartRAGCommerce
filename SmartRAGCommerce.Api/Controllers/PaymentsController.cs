using Microsoft.AspNetCore.Mvc;
using SmartRAGCommerce.Application.DTOs.Payments;
using SmartRAGCommerce.Application.Interfaces.Payments;

namespace SmartRAGCommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var payments = await _paymentService.GetByOrderIdAsync(orderId);
        return Ok(payments);
    }

    [HttpPost("order/{orderId:int}")]
    public async Task<IActionResult> Create(
        int orderId, CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreateAsync(orderId, dto);

            return CreatedAtAction(
                nameof(GetByOrderId),
                new { orderId },
                payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{paymentId:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int paymentId, UpdatePaymentStatusDto dto)
    {
        try
        {
            await _paymentService.UpdateStatusAsync(paymentId, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}