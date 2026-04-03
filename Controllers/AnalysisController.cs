using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly AnalysisService _analysisService;

    public AnalysisController(AnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetAnalysis(string userId)
    {
        var pdf = await _analysisService.GenerateAnalysis(userId);

        return File(pdf, "application/pdf", "analysis.pdf");
    }
}