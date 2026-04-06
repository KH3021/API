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

        // 🔥 USER GROWTH (DATE WISE)
        var userGrowth = clients
            .GroupBy(u => u.CreatedDate.Date)
            .Select(g => new
            {
                date = g.Key,
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToList();

        // 🔹 RESULTS
        var results = await _mongo.GetAllResults();

        var clientResults = results
            .Where(r => clients.Any(u => u.UserId == r.UserId))
            .ToList();

        int totalTests = clientResults.Count;

        double avg = totalTests == 0 ? 0 :
            clientResults.Average(r => r.Percentage);

        // 🔥 ACTIVE USERS
        int activeUsers = clientResults
            .Select(r => r.UserId)
            .Distinct()
            .Count();

        // 🔥 MOST ACTIVE USERS
        var mostActiveUsers = clientResults
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                userId = g.Key,
                attempts = g.Count()
            })
            .OrderByDescending(x => x.attempts)
            .Take(5)
            .ToList();

        // 🔹 SKILLS
        var skills = await _mongo.GetAllSkills();
        int totalSkills = skills.Count;

        var skillMap = skills.ToDictionary(
            s => s.SkillId,
            s => s.SkillName
        );

        // 🔥 TRENDING SKILLS (BASED ON RESULTS)
        var trendingSkills = clientResults
            .GroupBy(r => r.SkillId)
            .Select(g => new
            {
                skill = skillMap.TryGetValue(g.Key, out var name) ? name : g.Key,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToList();

        // 🔥 USER SKILLS (FOR ADDITIONAL INSIGHT)
        var userSkills = await _mongo.GetAllUserSkills();

        var clientUserSkills = userSkills
            .Where(us => clients.Any(u => u.UserId == us.UserId))
            .ToList();

        // 🔥 TOP SKILLS (BASED ON SELECTION)
        var topSkills = clientUserSkills
            .GroupBy(x => x.SkillId)
            .Select(g => new
            {
                skill = skillMap.TryGetValue(g.Key, out var name) ? name : g.Key,
                count = g.Count()
            })
            .OrderByDescending(x => x.count)
            .Take(5)
            .ToList();

        // 🔥 DROP RATE
        var threshold = DateTime.UtcNow.AddDays(-5);

        int inactiveUsers = clients.Count(u => u.LastLogin < threshold);

        double dropRate = totalUsers == 0 ? 0 :
            (inactiveUsers * 100.0 / totalUsers);

        // 🔹 FINAL RESPONSE
        return Ok(new
        {
            totalUsers,
            totalSkills,
            totalTests,
            averagePercentage = Math.Round(avg, 2),

            activeUsers,
            inactiveUsers,
            dropRate = Math.Round(dropRate, 2),

            trendingSkills,
            topSkills,
            mostActiveUsers,
            userGrowth
        });
    }
}