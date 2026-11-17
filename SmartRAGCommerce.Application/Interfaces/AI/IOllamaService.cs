namespace SmartRAGCommerce.Application.Interfaces.AI;

public interface IOllamaService
{
    Task<string> GenerateResponseAsync(string prompt);
}