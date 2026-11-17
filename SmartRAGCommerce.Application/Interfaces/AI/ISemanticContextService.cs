namespace SmartRAGCommerce.Application.Interfaces.AI;

public interface ISemanticContextService
{
    Task<string> GetContextAsync(string question);
}