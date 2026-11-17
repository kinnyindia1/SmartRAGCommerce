
using System.ComponentModel.DataAnnotations;

namespace SmartRAGCommerce.Application.DTOs.Analytics;

public class AnalyticsQueryRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Question { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
}
