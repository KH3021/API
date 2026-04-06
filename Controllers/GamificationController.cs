using Microsoft.AspNetCore.Mvc;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/gamification")]
public class GamificationController : ControllerBase
{
    private readonly GamificationService _service;

    public GamificationController(GamificationService service)
    {
        _service = service;
    }

    // ✅ Get user stats
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetStats(string userId)
    {
        var data = await _service.GetOrCreate(userId);

        var badge = data.Points switch
        {
            >= 200 => "Pro",
            >= 100 => "Intermediate",
            _ => "Beginner"
        };

        return Ok(new
        {
            data.Points,
            data.Level,
            data.CompletedTasks,
            Badge = badge
        });
    }
}