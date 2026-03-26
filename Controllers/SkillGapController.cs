using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillGapController : ControllerBase
{
    private readonly MongoService _mongo;

    public SkillGapController(MongoService mongo)
    {
        _mongo = mongo;
    }

    [HttpGet("{userId}/{skillId}")]
    public async Task<IActionResult> GetSkillGap(string userId, string skillId)
    {
        var results = await _mongo.GetResultsByUser(userId);

        var skillResults = results
            .Where(r => r.SkillId == skillId && r.Percentage == 100)
            .ToList();

        int beginner = skillResults.Count(r => r.Level.ToLower() == "beginner");
        int mid = skillResults.Count(r => r.Level.ToLower() == "mid");
        int expert = skillResults.Count(r => r.Level.ToLower() == "expert");

        beginner = Math.Min(beginner, 40);
        mid = Math.Min(mid, 40);
        expert = Math.Min(expert, 40);

        double beginnerPct = (double)beginner / 40 * 100;
        double midPct = (double)mid / 40 * 100;
        double expertPct = (double)expert / 40 * 100;

        double completion = (beginnerPct + midPct + expertPct) / 3;
        double gap = 100 - completion;

        var response = new SkillGapModel
        {
            UserId = userId,
            SkillId = skillId,

            BeginnerPerfect = beginner,
            MidPerfect = mid,
            ExpertPerfect = expert,

            CompletionPercentage = Math.Round(completion, 2),
            SkillGapPercentage = Math.Round(gap, 2),

            IsMastered = (beginner >= 40 && mid >= 40 && expert >= 40)
        };

        return Ok(response);
    }
}