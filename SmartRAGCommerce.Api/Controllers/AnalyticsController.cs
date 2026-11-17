
using Microsoft.AspNetCore.Mvc;
using SmartRAGCommerce.Application.DTOs.Analytics;
using SmartRAGCommerce.Application.Interfaces.Analytics;

namespace SmartRAGCommerce.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;
    public AnalyticsController( IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AnalyticsQueryRequestDto request)
    {
        try
        {
            var result = await _analyticsService.AskAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analytics API failed");

            return StatusCode(500, new
            {
                message = ex.Message,
                details = ex.ToString()
            });
        }
    }
}
