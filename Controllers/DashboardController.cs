using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly MongoService _mongo;

    public DashboardController(MongoService mongo)
    {
        _mongo = mongo;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetDashboard(string userId)
    {
        // ✅ Get user skills
        var userSkills = await _mongo.GetUserSkills(userId);

        // ✅ Get all results
        var results = await _mongo.GetResultsByUser(userId);

        int totalTests = results.Count;
        int totalSkills = userSkills.Count;

        double avg = totalTests == 0 ? 0 :
            results.Average(r => r.Percentage);

        var skillProgress = new List<object>();

        // 🔥 For each skill → calculate gap
        foreach (var skill in userSkills)
        {
            var skillResults = results
                .Where(r => r.SkillId == skill.SkillId && r.Percentage == 100)
                .ToList();

            int beginner = Math.Min(skillResults.Count(r => r.Level.ToLower() == "beginner"), 40);
            int mid = Math.Min(skillResults.Count(r => r.Level.ToLower() == "mid"), 40);
            int expert = Math.Min(skillResults.Count(r => r.Level.ToLower() == "expert"), 40);

            double completion = (
                ((double)beginner / 40 * 100) +
                ((double)mid / 40 * 100) +
                ((double)expert / 40 * 100)
            ) / 3;

            skillProgress.Add(new
            {
                SkillId = skill.SkillId,
                Completion = Math.Round(completion, 2),
                Gap = Math.Round(100 - completion, 2),
                IsMastered = (beginner >= 40 && mid >= 40 && expert >= 40)
            });
        }

        var dashboard = new DashboardModel
        {
            UserId = userId,
            TotalSkills = totalSkills,
            TotalTests = totalTests,
            AveragePercentage = Math.Round(avg, 2),
            SkillProgress = skillProgress
        };

        return Ok(dashboard);
    }
}