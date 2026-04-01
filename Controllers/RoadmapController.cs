using Microsoft.AspNetCore.Mvc;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly RoadmapService _roadmapService;

    public RoadmapController(RoadmapService roadmapService)
    {
        _roadmapService = roadmapService;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> Generate(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new
                {
                    message = "UserId is required"
                });

            var result = await _roadmapService.GenerateRoadmap(userId);

            if (result == null)
                return NotFound(new
                {
                    message = "User not found"
                });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Something went wrong",
                error = ex.Message
            });
        }
    }
}