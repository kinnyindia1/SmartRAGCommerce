
using System.ComponentModel.DataAnnotations;

namespace SmartRAGCommerce.Application.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    [RegularExpression("^(Pending|Completed|Cancelled)$")]
    public string Status { get; set; } = string.Empty;
}
