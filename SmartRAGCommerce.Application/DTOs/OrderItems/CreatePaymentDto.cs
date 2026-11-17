
using System.ComponentModel.DataAnnotations;

namespace SmartRAGCommerce.Application.DTOs.Payments;

public class CreatePaymentDto
{
    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;
}
