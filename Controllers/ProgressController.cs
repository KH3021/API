using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ProgressService _service;

    public ProgressController(ProgressService service)
    {
        _service = service;
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

        return Ok(result);
    }
}