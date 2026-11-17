
namespace SmartRAGCommerce.Application.Interfaces.Analytics;

public interface ISmartRagQueryService
{
    Task<string> GetContextAsync(string question);
}
