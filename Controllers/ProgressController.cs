using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ProgressService _service;
    private readonly NotificationService _notification;

    public ProgressController(ProgressService service, NotificationService notification)
    {
        _service = service;
        _notification = notification;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] ProgressModel request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.SkillId))
            return BadRequest("UserId and SkillId required");

        var result = await _service.CalculateProgress(
            request.UserId,
            request.SkillId,
            request.Score
        );

        // 🔔 SEND NOTIFICATION
        await _notification.SendOrUpdateNotification(
            request.UserId,
            "Your progress has been updated 📈"
        );

        return Ok(result);
    }
}