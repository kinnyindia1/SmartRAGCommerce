namespace SmartRAGCommerce.Application.Interfaces.Analytics;

public interface IAnalyticsQueryExecutor
{
    Task<List<Dictionary<string, object?>>> ExecuteReadOnlyAsync(string sql);
}