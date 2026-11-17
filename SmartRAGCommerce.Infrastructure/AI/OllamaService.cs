using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SmartRAGCommerce.Application.Interfaces.AI;

namespace SmartRAGCommerce.Infrastructure.AI;
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _model;
    public OllamaService( HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["Ollama:BaseUrl"] ?? throw new InvalidOperationException("Ollama BaseUrl is missing.");
        _endpoint = baseUrl.TrimEnd('/') + (configuration["Ollama:ChatEndpoint"] ?? "/v1/chat/completions");
        _model = configuration["Ollama:Model"]?? throw new InvalidOperationException("Ollama Model is missing.");
        var timeout = int.TryParse( configuration["Ollama:TimeoutSeconds"], out var seconds) ? seconds : 600;
        _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
    }
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.1
        };
        using var response = await _httpClient.PostAsJsonAsync( _endpoint, request);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse( await response.Content.ReadAsStringAsync());
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }
}