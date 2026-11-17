
using SmartRAGCommerce.Application.DTOs.Customers;
using SmartRAGCommerce.Application.Interfaces.Customers;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;

namespace SmartRAGCommerce.Application.Services.Customers;
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }
    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();
        return customers.Select(MapToDto).ToList();
    }
    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer == null ? null : MapToDto(customer);
    }
    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
    {
        if (await _repository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already exists.");
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address
        };
        var savedCustomer = await _repository.AddAsync(customer);
        return MapToDto(savedCustomer);
    }
    public async Task<bool> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            return false;
        if (await _repository.EmailExistsAsync(dto.Email, id))
            throw new InvalidOperationException("Email already exists.");
        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;
        customer.PhoneNumber = dto.PhoneNumber;
        customer.Address = dto.Address;
        await _repository.UpdateAsync(customer);
        return true;
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null)
            return false;
        await _repository.DeleteAsync(customer);
        return true;
    }
    private static CustomerResponseDto MapToDto(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }
}
