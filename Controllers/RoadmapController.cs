using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly RoadmapService _service;

    public RoadmapController(RoadmapService service)
    {
        _service = service;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> Generate(string userId, string skillId, string level)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(skillId))
            return BadRequest("UserId and SkillId required");

        var roadmap = await _service.GenerateRoadmap(userId, skillId, level);

        if (roadmap == null)
            return NotFound("Skill not found");

        return Ok(roadmap);
    }
}