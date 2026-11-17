using SmartRAGCommerce.Application.DTOs.Analytics;

namespace SmartRAGCommerce.Application.Interfaces.Analytics;

public interface IAnalyticsService
{
    Task<AnalyticsQueryResponseDto> AskAsync(AnalyticsQueryRequestDto request);
}