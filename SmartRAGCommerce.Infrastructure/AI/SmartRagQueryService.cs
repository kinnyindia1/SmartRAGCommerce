
using Microsoft.Extensions.Logging;
using SmartRAG.Interfaces.Document;
using SmartRAG.Models.RequestResponse;
using SmartRAGCommerce.Application.Interfaces.Analytics;

namespace SmartRAGCommerce.Infrastructure.AI;

public class SmartRagQueryService : ISmartRagQueryService
{
    private readonly IDocumentSearchService _documentSearchService;
    private readonly ILogger<SmartRagQueryService> _logger;

    public SmartRagQueryService(
        IDocumentSearchService documentSearchService,
        ILogger<SmartRagQueryService> logger)
    {
        _documentSearchService = documentSearchService;
        _logger = logger;
    }

    public async Task<string> GetContextAsync(string question)
    {
        try
        {
            var request = new QueryIntelligenceRequest
            {
                Query = $"-d {question}",
                MaxResults = 5
            };

            var result = await _documentSearchService.QueryIntelligenceAsync(
                request,
                CancellationToken.None);

            _logger.LogInformation("SmartRAG document query completed.");

            return result.Answer ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SmartRAG document query failed.");
            throw;
        }
    }
}
