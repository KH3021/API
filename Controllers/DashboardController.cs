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

    // ADMIN DASHBOARD
    [HttpGet("admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        // Get ALL users
        var users = await _mongo.GetAllUsers();

        // Filter ONLY Clients
        var clients = users
            .Where(u => u.Role != null && u.Role.ToLower() == "client")
            .ToList();

        int totalUsers = clients.Count;

        // Get ALL results
        var allResults = await _mongo.GetAllResults();

        // Only results of client users
        var clientResults = allResults
            .Where(r => clients.Any(u => u.UserId == r.UserId))
            .ToList();

        int totalTests = clientResults.Count;

        double avg = totalTests == 0 ? 0 :
            clientResults.Average(r => r.Percentage);

        // Get ALL skills
        var skills = await _mongo.GetAllSkills();
        int totalSkills = skills.Count;

        // FINAL RESPONSE
        return Ok(new
        {
            totalUsers,
            totalSkills,
            totalTests,
            averagePercentage = Math.Round(avg, 2)
        });
    }
}