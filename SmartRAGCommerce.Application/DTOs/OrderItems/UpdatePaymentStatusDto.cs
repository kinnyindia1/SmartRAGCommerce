
using System.ComponentModel.DataAnnotations;

namespace SmartRAGCommerce.Application.DTOs.Payments;

public class UpdatePaymentStatusDto
{
    [Required]
    [RegularExpression("^(Pending|Completed|Failed|Refunded)$")]
    public string PaymentStatus { get; set; } = string.Empty;
}
