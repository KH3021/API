using Microsoft.AspNetCore.Mvc;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudyPlannerController : ControllerBase
{
    private readonly StudyPlannerService _service;

    public StudyPlannerController(StudyPlannerService service)
    {
        _service = service;
    }

    [HttpGet("generate/{userId}")]
    public async Task<IActionResult> Generate(string userId)
    {
        var plan = await _service.GeneratePlan(userId);
        return Ok(plan);
    }
}