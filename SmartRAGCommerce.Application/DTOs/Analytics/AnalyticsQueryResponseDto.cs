
namespace SmartRAGCommerce.Application.DTOs.Analytics;

public class AnalyticsQueryResponseDto
{
    public string SessionId { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public List<Dictionary<string, object?>> Data { get; set; } = new();

    public int RowCount { get; set; }
}
