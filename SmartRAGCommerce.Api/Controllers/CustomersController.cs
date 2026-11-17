
using Microsoft.AspNetCore.Mvc;
using SmartRAGCommerce.Application.DTOs.Customers;
using SmartRAGCommerce.Application.Interfaces.Customers;

namespace SmartRAGCommerce.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _customerService.GetAllAsync();
        return Ok(customers);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found." });
        return Ok(customer);
    }
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        try
        {
            var customer = await _customerService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                customer);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCustomerDto dto)
    {
        try
        {
            var updated = await _customerService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = "Customer not found." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = "Customer not found." });
        return NoContent();
    }
}
