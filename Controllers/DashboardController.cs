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

    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        // 🔹 USERS
        var users = await _mongo.GetAllUsers();

        var clients = users
            .Where(u => !string.IsNullOrEmpty(u.Role) && u.Role.ToLower() == "client")
            .ToList();

        int totalUsers = clients.Count;

        // 🔹 RESULTS
        var results = await _mongo.GetAllResults();

        var clientResults = results
            .Where(r => clients.Any(u => u.UserId == r.UserId))
            .ToList();

        int totalTests = clientResults.Count;

        double avg = totalTests == 0 ? 0 :
            clientResults.Average(r => r.Percentage);

        // 🔹 SKILLS
        var skills = await _mongo.GetAllSkills();
        int totalSkills = skills.Count;

        // 🔥 SKILL MAP (FAST LOOKUP)
        var skillMap = skills.ToDictionary(
            s => s.SkillId,
            s => s.SkillName
        );

        // 🔥 ACTIVE USERS
        int activeUsers = clientResults
            .Select(r => r.UserId)
            .Distinct()
            .Count();

        // 🔥 USER SKILLS
        var userSkills = await _mongo.GetAllUserSkills();

        // 🔥 FILTER ONLY CLIENT USERS
        var clientUserSkills = userSkills
            .Where(us => clients.Any(u => u.UserId == us.UserId))
            .ToList();

        // 🔥 TOP SKILLS
        var topSkills = clientUserSkills
            .GroupBy(x => x.SkillId)
            .Select(g => new
            {
                SkillId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        // 🔥 MAP SkillId → SkillName (SAFE)
        var topSkillsWithNames = topSkills.Select(ts => new
        {
            skill = skillMap.TryGetValue(ts.SkillId, out var name)
                ? name
                : ts.SkillId,
            count = ts.Count
        }).ToList();

        // 🔹 FINAL RESPONSE
        return Ok(new
        {
            totalUsers,
            totalSkills,
            totalTests,
            averagePercentage = Math.Round(avg, 2),

            activeUsers,
            topSkills = topSkillsWithNames
        });
    }
}   