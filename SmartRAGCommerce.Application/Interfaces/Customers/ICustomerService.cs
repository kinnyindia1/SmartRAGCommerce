using SmartRAGCommerce.Application.DTOs.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRAGCommerce.Application.Interfaces.Customers
{
    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllAsync();

        Task<CustomerResponseDto?> GetByIdAsync(int id);

        Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto);

        Task<bool> UpdateAsync(int id, UpdateCustomerDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
