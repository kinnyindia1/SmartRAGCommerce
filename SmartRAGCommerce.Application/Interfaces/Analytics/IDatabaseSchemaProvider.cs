namespace SmartRAGCommerce.Application.Interfaces.Analytics;

public interface IDatabaseSchemaProvider
{
    Task<string> GetSchemaDescriptionAsync();
}