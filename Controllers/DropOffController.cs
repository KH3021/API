using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DropOffController : ControllerBase
{
    private readonly MongoService _mongo;

    public DropOffController(MongoService mongo)
    {
        _mongo = mongo;
    }

    [HttpGet("at-risk")]
    public async Task<IActionResult> GetAtRiskUsers()
    {
        var users = await _mongo.GetAllUsers();

        var threshold = DateTime.UtcNow.AddDays(-5);

        var atRiskUsers = users
            .Where(u => u.LastLogin < threshold)
            .ToList();

        // 🔥 GET YOUR EXISTING N001 TEMPLATE
        var template = await _mongo.Notifications
            .Find(n => n.Nid == "N001")
            .FirstOrDefaultAsync();

        if (template == null)
            return BadRequest("N001 not found ❌");

        foreach (var user in atRiskUsers)
        {
            // ✅ CHECK DUPLICATE USING NID + USERID
            var existing = await _mongo.Notifications
                .Find(n => n.UserId == user.UserId && n.Nid == "N001")
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var notification = new Notification
                {
                    Nid = "N001", // 🔥 STATIC
                    UserId = user.UserId,
                    Message = template.Message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _mongo.AddNotification(notification);
            }
        }

        return Ok(new
        {
            count = atRiskUsers.Count,
            users = atRiskUsers
        });
    }
}